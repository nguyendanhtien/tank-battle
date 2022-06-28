#include <pthread.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <poll.h>
#include <fcntl.h>
#include <sys/types.h> 
#include <sys/socket.h>
#include <netinet/in.h>
#include "queue.h"
#include <time.h>

#define MAX_MSG_LEN 128
#define USER_CAPACITY 256
#define MAX_GAME_TIME 180
#define MAX_POWER_TIME 10
#define MAX_HP 10
struct PlayerStruct {
    int coord[3];
    int hp;
    int is_shot;
    int power_elapsed;
};

typedef struct PlayerStruct Player;

int setup_listener(int portno);
int matching_request_handler(char *message, int msg_len, int socket);
void *client_connection_handler(void *cli_sockfd);
void send_client_msg(int cli_sockfd, char * msg);
void send_clients_msg(int cli_sockfd1, int cli_sockfd2, char * msg);
void *run_game(void *room_info);
Player* player_init();
void reset_stats(Player* p1, Player* p2);
int max(int a, int b);
int min(int a, int b);
int player_count = 0;
int game_count = 0;
int players[USER_CAPACITY];

pthread_mutex_t mutexcount;
pthread_mutex_t mutex_game_count;
pthread_mutex_t mutex_room_queue;
pthread_mutex_t mutex_player[USER_CAPACITY];
pthread_t client_thread[USER_CAPACITY];
pthread_t game_thread[USER_CAPACITY];

queue *roomQueue;

int main(int argc, char *argv[]) {   
    if (argc < 2) {
        fprintf(stderr,"ERROR, no port provided\n");
        exit(1);
    }


    int lis_sockfd = setup_listener(atoi(argv[1])); 
    pthread_mutex_init(&mutexcount, NULL);
    pthread_mutex_init(&mutex_room_queue, NULL);
    pthread_mutex_init(&mutex_game_count, NULL);
    roomQueue = s_createQueue();

    while (1) {
        if (player_count < USER_CAPACITY) {   

            int cli_sockfd;
            socklen_t clilen;
            struct sockaddr_in serv_addr, cli_addr;

            listen(lis_sockfd, USER_CAPACITY - player_count);

            memset(&cli_addr, 0, sizeof(cli_addr));

            clilen = sizeof(cli_addr);
        
            cli_sockfd = accept(lis_sockfd, (struct sockaddr *) &cli_addr, &clilen);


            if (cli_sockfd < 0)
                error("ERROR accepting a connection from a client.");


            pthread_mutex_lock(&mutexcount);
            player_count++;
            printf("[DEBUG]Number of players is now %d.\n", player_count);
            
            
            // pthread_t client_thread;
            int client_thread_ret = pthread_create(&client_thread[player_count - 1], NULL, client_connection_handler, &cli_sockfd);
            if (client_thread_ret) {
                printf("Thread for client %d creation failed with return code %d\n", cli_sockfd, client_thread_ret);
                exit(-1);
            }
            pthread_mutex_unlock(&mutexcount);
        }
    }

    for (int i = 0; i < player_count; i++) {
	    pthread_join(client_thread[i], NULL);
	}

    for (int i = 0; i < game_count; i++) {
	    pthread_join(game_thread[i], NULL);
	}


    close(lis_sockfd);

    pthread_mutex_destroy(&mutexcount);
    pthread_mutex_destroy(&mutex_game_count);
    for (int i = 0; i < USER_CAPACITY; i++) {
	    pthread_mutex_destroy(&mutex_player[i]);
	}
    pthread_mutex_destroy(&mutex_room_queue);
    pthread_exit(NULL);
}


Player* player_init(){
    struct PlayerStruct* temp = (struct PlayerStruct*)malloc(sizeof(struct PlayerStruct));
    temp->hp = MAX_HP;
    temp->coord[0] = 0;
    temp->coord[1] = 0;
    temp->coord[2] = 0;
    temp->is_shot = 0;
    temp->power_elapsed = 0;
    return temp;
}

void reset_stats(Player* p1, Player* p2) {
    p1->is_shot = 0;
    p2->is_shot = 0;
}

int max(int a, int b) {
    if (a >= b) 
        return a;
    else 
        return b;
}

int min(int a, int b) {
    if (a <= b) 
        return a;
    else 
        return b;
}

int calc_time(time_t begin) {
    time_t end = time(0);
    // printf("Time spend: %ld %ld", end, end-begin);
    return (int) (end - begin);
    // return (int)time_spent;
}
void *run_game(void *room_info) {

    
    struct Room room = *(struct Room*) room_info;
    int client1 = room.client1;
    int client2 = room.client2;
    
    Player* p1 = player_init();
    Player* p2 = player_init();
    
    
    printf("---TANK BATTLE---\n");
    printf("  Room: %d\n- Client1: %d - Client2: %d\n- Ready: %d - Start: %d.\n", room.key, room.client1, room.client2, room.is_ready, room.is_start);
    
    struct pollfd pfds[2];

    pfds[0].fd = client1;
	pfds[0].events = POLLIN;
    pfds[1].fd = client2;
	pfds[1].events = POLLIN;
    
    int poll_ret;
    int msg_len;
    char items_state[6] = "11111";
    int time_elapsed = MAX_GAME_TIME;
    char buff[MAX_MSG_LEN];
    bzero(buff, MAX_MSG_LEN);
    int gameover = 0;
    int gameend = 0;
    time_t begin, power_clock1, power_clock2;
    int is_cont[2] = {-1, -1};
    begin = time(0);
    power_clock1 = time(0);
    power_clock2 = time(0);
  
    int power_elapsed;
    while (!gameend) {

		poll_ret = poll(pfds, 2, 500);
		switch (poll_ret) {
			case 0:
				break;                                                    
			case -1:
				perror("Error on poll");
			default:   
				for(int i = 0; i < 2; i++ )	{

					if( pfds[i].revents & POLLIN ) {
                        // clock_t end = clock();
                        time_elapsed = max(180 - calc_time(begin), 0);

                        if (time_elapsed <= 0 && gameover == 0) {
                            if (p1->hp <= p2->hp) {
                                send_client_msg(pfds[1].fd, "\nENDS:WON");
                                send_client_msg(pfds[0].fd, "\nENDS:LOSE");
                                gameover = 1;
                            } else {
                                send_client_msg(pfds[0].fd, "\nENDS:WON");
                                send_client_msg(pfds[1].fd, "\nENDS:LOSE");
                                gameover = 1;
                            }
                        }
						power_elapsed = calc_time(power_clock1);
                        if (p1->power_elapsed != 0)
                            p1->power_elapsed = max(MAX_POWER_TIME - power_elapsed, 0);
                        power_elapsed = calc_time(power_clock2);
                        if (p2->power_elapsed != 0)
                            p2->power_elapsed = max(MAX_POWER_TIME - power_elapsed, 0);
                        
						msg_len = recv(pfds[i].fd, buff, MAX_MSG_LEN, 0);

                        if (msg_len <= 0) {
                            printf("[DEBUG]Player %d disconnected.\n", pfds[i].fd);
                            bzero(buff, MAX_MSG_LEN);
                            msg_len = recv(pfds[(i + 1) % 2].fd, buff, MAX_MSG_LEN, 0);
                            if (msg_len > 0) {
                                send_client_msg(pfds[(i + 1) % 2].fd, "\nENDS:WINA");
                                send_client_msg(pfds[(i + 1) % 2].fd, "\nHOME");
                            } else {
                                printf("[DEBUG]Player %d disconnected.\n", pfds[(i + 1) % 2].fd);
                            }
                            gameend = 1;
                            break;
                        }
                        
                        buff[msg_len] = '\0';
                        printf("[IN-GAME] Client %d: %s\n", pfds[i].fd, buff);

                        char msg_type[5];

                        strncpy(msg_type, &buff[0], 4);
                        msg_type[4] = '\0';

                        if (!strcmp(msg_type, "CONT")) {
                            char item_id_str[2];
                            strncpy(item_id_str, &buff[5], 1);
                            item_id_str[1] = '\0';
                            int item_id = atoi(item_id_str);
                            is_cont[i] = item_id;

                            if (is_cont[0] == 0 || is_cont[1] == 0) {
                                gameend = 1;
                                send_clients_msg(pfds[0].fd, pfds[1].fd, "HOME");
                                break;
                            } else if (is_cont[0] == 1 && is_cont[1] == 1 && gameover == 1) {
                                printf("[DEBUG]GAME RESTART\n");
                                printf("---TANK BATTLE---\n");
                                printf("  Room: %d\n- Client1: %d - Client2: %d\n- Ready: %d - Start: %d.\n", room.key, room.client1, room.client2, room.is_ready, room.is_start);
                                p1 = player_init();
                                p2 = player_init();
                                begin = time(0);
                                power_clock1 = time(0);
                                power_clock2 = time(0);
                                bzero(buff, MAX_MSG_LEN);
                                gameover = 0;
                                gameend = 0;
                                is_cont[0] = -1;
                                is_cont[1] = -1;
                                sprintf(items_state, "11111");
                                break;
                            } else
                                continue;
                        }

                        

                        if (!strcmp(msg_type, "MOVE")) {
                            char coords_str[msg_len-4];
                            strncpy(coords_str, &buff[5], msg_len-5);
                            coords_str[msg_len-4] = '\0';
                            char * token = strtok(coords_str, "-");
                            // loop through the string to extract all other tokens
                            int j = 0;
                            while( token != NULL ) {
                                if (i == 0) {
                                    p1->coord[j] = atoi(token);
                                } else {
                                    p2->coord[j] = atoi(token);
                                }
                                token = strtok(NULL, "-");
                                j++;
                            }
                        }

                        if (!strcmp(msg_type, "GHIT")) {
                            if (i == 0) {
                                if (p2->power_elapsed)
                                    p1->hp -= 3;
                                else
                                    p1->hp--;
                                p1->hp = max(p1->hp, 0);
                            } else {
                                if (p1->power_elapsed)
                                    p2->hp -= 3;
                                else
                                    p2->hp--;
                                p2->hp = max(p2->hp, 0);
                            }
                        }

                        if (!strcmp(msg_type, "SHOT")) {
                            if (i == 0) {
                                p1->is_shot = 1;
                            } else {
                                p2->is_shot = 1;
                            }
                        }

                        if (!strcmp(msg_type, "UPHP")) {
                            char item_id_str[2];
                            strncpy(item_id_str, &buff[5], 1);
                            item_id_str[1] = '\0';
                            int item_id = atoi(item_id_str);
                            if (i == 0) {
                                p1->hp++;
                                p1->hp = min(p1->hp, MAX_HP);
                            } else {
                                p2->hp++;
                                p2->hp = min(p2->hp, MAX_HP);
                            }
                            items_state[item_id] = '0';
                        }

                        if (!strcmp(msg_type, "UPAT")) {
                            char item_id_str[2];
                            strncpy(item_id_str, &buff[5], 1);
                            item_id_str[1] = '\0';
                            int item_id = atoi(item_id_str);
                            if (i == 0) {
                                p1->power_elapsed = MAX_POWER_TIME;
                                power_clock1 = time(0);
                            } else {
                                p2->power_elapsed = MAX_POWER_TIME;
                                power_clock2 = time(0);
                            }
                            items_state[item_id] = '0';
                        }


                        char state[MAX_MSG_LEN];

                        sprintf(state, "STAT:P1:%d,%d,%d-%d-%d-%d|P2:%d,%d,%d-%d-%d-%d|ITEMS:%s|TIME:%d", 
                                p1->coord[0], p1->coord[1], p1->coord[2], p1->hp, p1->is_shot, p1->power_elapsed,
                                p2->coord[0], p2->coord[1], p2->coord[2], p2->hp, p2->is_shot, p2->power_elapsed,
                                items_state,
                                time_elapsed);

                        printf("%s\n", state);
                        send_clients_msg(pfds[0].fd, pfds[1].fd, state);
                        
                        if (p1->hp <= 0) {
                            send_client_msg(pfds[1].fd, "\nENDS:WON");
                            send_client_msg(pfds[0].fd, "\nENDS:LOSE");
                            gameover = 1;
                        }
                        if (p2->hp <= 0) {
                            send_client_msg(pfds[0].fd, "\nENDS:WON");
                            send_client_msg(pfds[1].fd, "\nENDS:LOSE");
                            gameover = 1;
                        }
                        
                        reset_stats(p1, p2);
                        
                        bzero(buff, MAX_MSG_LEN);
                        if (gameover) {
                            send_clients_msg(pfds[0].fd, pfds[1].fd, "\nCONT");
                        }
					}
				} 
		} 
	}
    pthread_mutex_lock(&mutex_room_queue);
    int roomId = s_delete_node(roomQueue, room.client1);
    pthread_mutex_unlock(&mutex_room_queue);

    printf("[DEBUG]Delete room: %d.\n", roomId);


    pthread_mutex_unlock(&mutex_player[pfds[0].fd]);
    pthread_mutex_unlock(&mutex_player[pfds[1].fd]);


    pthread_exit(NULL);
    return 0;
}   

void create_game_thread(){
  
    pthread_mutex_lock(&mutex_room_queue);

    struct Room* temp = roomQueue->front;

    while (temp != NULL) {
        
        if (temp->is_ready == 1 & temp->is_start == 0) {

            temp->is_start = 1;

            struct Room* room = (struct Room*)malloc(sizeof(struct Room));
            room = temp;
            pthread_mutex_lock(&mutex_player[temp->client1]);
            players[temp->client1] = 1;
            pthread_mutex_lock(&mutex_player[temp->client2]);
            players[temp->client2] = 1;
            
            pthread_mutex_lock(&mutex_game_count);
            
            game_count++;
            
            int game_thread_ret = pthread_create(&game_thread[game_count - 1], NULL, run_game, room);
            
            pthread_mutex_unlock(&mutex_game_count);
            
            if (game_thread_ret) {
                printf("Thread for game %d creation failed with return code %d\n", room->key, game_thread_ret);
                exit(-1);
            }
            
        }

        temp = temp->next;
    }

    pthread_mutex_unlock(&mutex_room_queue);
}

void *client_connection_handler(void *cli_sockfd) {

    int socket = *(int*) cli_sockfd;
	int read_len;

    char hello[MAX_MSG_LEN] = "Hello from server";
	send(socket, hello , strlen(hello),0);	
	
    
    char client_message[MAX_MSG_LEN];
    bzero(client_message, MAX_MSG_LEN);

	while(1) {
        pthread_mutex_lock(&mutex_player[socket]);
        players[socket] = 0;
        pthread_mutex_unlock(&mutex_player[socket]);
        if ((read_len = recv(socket, client_message, MAX_MSG_LEN, 0)) <= 0)
            break;
		client_message[read_len] = '\0';

        printf("\nClient %d: %s\n", socket, client_message);

		if (matching_request_handler(client_message, read_len, socket) == 1) {
            create_game_thread();
            // break;
        }	

	}
    if (read_len <= 0) 
        printf("[DEBUG]Player %d is disconnected.\n", socket);

	pthread_exit(NULL);
	return 0;
}

int matching_request_handler(char *message, int msg_len, int socket) {
    
    char msg_type[5];

    strncpy(msg_type, &message[0], 4);
    msg_type[4] = '\0';

   
    if (!strcmp(msg_type, "DROP")) {
        printf("[DEBUG]Player %d cancel waiting opponent.\n", socket);

        pthread_mutex_lock(&mutex_room_queue);
        int roomId = s_delete_node(roomQueue, socket);
        pthread_mutex_unlock(&mutex_room_queue);

        printf("[DEBUG]Delete room: %d.\n", roomId);

        send_client_msg(socket, "DROP:OK");

        return 0;

    } else if (!strcmp(msg_type, "CREA")) {

        srand(time(NULL));   
        int room_id = rand() % 1000;  

        pthread_mutex_lock(&mutex_room_queue);
        s_enqueue(roomQueue, room_id, socket);
        pthread_mutex_unlock(&mutex_room_queue);

        printf("[DEBUG]Created Room: %d for player %d\n", room_id, socket);
        char send_message[MAX_MSG_LEN];
        sprintf(send_message, "CREA:%d", room_id);
        send_client_msg(socket, send_message);

        return 0;

    } else if (!strcmp(msg_type, "JOIN")) {

        char room_id_str[4];
        strncpy(room_id_str, &message[5], 3);
        room_id_str[3] = '\0';
        int room_id = atoi(room_id_str);

        int client1;

        pthread_mutex_lock(&mutex_room_queue);

        struct Room* temp = roomQueue->front;
        int is_room_available = 0;
       
        while (temp != NULL) {
            // printf("Room: %d", temp->key);
            if (temp->key == room_id && temp->is_ready == 0 & temp->is_start == 0) {
                is_room_available = 1;
                temp->is_ready = 1;
                temp->client2 = socket;
                client1 = temp->client1;
                break;
            }
            temp = temp->next;
        }
        pthread_mutex_unlock(&mutex_room_queue);

        if (is_room_available) {
            printf("[DEBUG]Match client %d and client %d with room %d successfully.\n", client1, socket, room_id);
            char send_message[MAX_MSG_LEN];
            sprintf(send_message, "MATC:%d-2", room_id);
            send_client_msg(socket, send_message);
            
            sprintf(send_message, "MATC:%d-1", room_id);
            send_client_msg(client1, send_message);
        } else {
            printf("[DEBUG]No available room with id: %d.\n", room_id);
            send_client_msg(socket, "JOIN:FAIL");
        }

        return 0;

    } else if (!strcmp(msg_type, "LIST")) {

        pthread_mutex_lock(&mutex_room_queue);
        struct Room* temp = roomQueue->front;

        if (temp == NULL) 
            printf("[DEBUG]No room available.\n");

        while (temp != NULL) {
            printf("  Room: %d\n- Client1: %d - Client2: %d\n- Ready: %d - Start: %d.\n", temp->key, temp->client1, temp->client2, temp->is_ready, temp->is_start);
            temp = temp->next;
        }
        pthread_mutex_unlock(&mutex_room_queue);

        send_client_msg(socket, "LIST:OK");
        
        return 0;

    } else if (!strcmp(msg_type, "PLAY")) {

        send_client_msg(socket, "PLAY:OK");

        return 1;

    } else {

        send_client_msg(socket, "IVLD");

        return 0;
    }
            
    
}

void send_client_msg(int cli_sockfd, char * msg){
    int n = send(cli_sockfd, msg, strlen(msg), 0);
    if (n < 0)
        error("ERROR writing msg to client socket");
}

void send_clients_msg(int cli_sockfd1, int cli_sockfd2, char * msg){
    int n = send(cli_sockfd1, msg, strlen(msg), 0);
    if (n < 0)
        error("ERROR writing msg to client socket");
    n = send(cli_sockfd2, msg, strlen(msg), 0);
    if (n < 0)
        error("ERROR writing msg to client socket");
}


int setup_listener(int portno) {
    int sockfd;
    struct sockaddr_in serv_addr;

    sockfd = socket(AF_INET, SOCK_STREAM, 0);
    if (sockfd < 0) 
        error("ERROR opening listener socket.");
    
    bzero(&serv_addr, sizeof(serv_addr));
    serv_addr.sin_family = AF_INET;	
    serv_addr.sin_addr.s_addr = INADDR_ANY;	
    serv_addr.sin_port = htons(portno);		

    if (bind(sockfd, (struct sockaddr *) &serv_addr, sizeof(serv_addr)) < 0)
        error("ERROR binding listener socket.");

    #ifdef DEBUG
    printf("[DEBUG] Listener set.\n");    
    #endif 

    return sockfd;
}