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

#define MAX_MSG_LEN 64
#define USER_CAPACITY 256

int setup_listener(int portno);
int matching_request_handler(char *message, int msg_len, int socket);
void *client_connection_handler(void *cli_sockfd);
void send_client_msg(int cli_sockfd, char * msg);
void send_clients_msg(int cli_sockfd1, int cli_sockfd2, char * msg);
void *run_game(void *room_info);

int player_count = 0;
int game_count = 0;

pthread_mutex_t mutexcount;
pthread_mutex_t mutex_game_count;
pthread_mutex_t mutex_room_queue;

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
            printf("Number of players is now %d.\n", player_count);
            
            
            // pthread_t client_thread;
            int client_thread_ret = pthread_create(&client_thread[player_count - 1], NULL, client_connection_handler, &cli_sockfd);
            if (client_thread_ret) {
                printf("Thread for client %d creation failed with return code %d\n", cli_sockfd, client_thread_ret);
                exit(-1);
            }
            pthread_mutex_unlock(&mutexcount);
            #ifdef DEBUG
            printf("[DEBUG] Starting new game thread...\n");
            #endif

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
    pthread_mutex_destroy(&mutex_room_queue);
    pthread_exit(NULL);
}

void *run_game(void *room_info) {
    
    struct Room room = *(struct Room*) room_info;
    int client1 = room.client1;
    int client2 = room.client2;
    
    int h1 = 5;
    int h2 = 5;
    
    
    printf("---TANK BATTLE---\n");
    printf("  Room: %d\n- Client1: %d - Client2: %d\n- Ready: %d - Start: %d.\n", room.key, room.client1, room.client2, room.is_ready, room.is_start);
    
    struct pollfd pfds[2];

    pfds[0].fd = client1;
	pfds[0].events = POLLIN;
    pfds[1].fd = client2;
	pfds[1].events = POLLIN;
    
    int poll_ret;
    int msg_len;
    char buff[MAX_MSG_LEN];
    bzero(buff, MAX_MSG_LEN);
    int gameover = 0;
    while (!gameover) {

		poll_ret = poll(pfds, 2, 500);
		switch (poll_ret) {
			case 0:
				break;                                                    
			case -1:
				perror("Error on poll");
			default:   
				for(int i = 0; i < 2; i++ )	{

					if( pfds[i].revents & POLLIN ) {
						
						msg_len = recv(pfds[i].fd, buff, MAX_MSG_LEN, 0);

                        if (msg_len <= 0) {
                            printf("Player %d disconnected.\n", pfds[i].fd);
                            gameover = 1;
                            break;
                        }
                        
                        buff[msg_len] = '\0';
                        printf("[IN-GAME] Client %d: %s\n", pfds[i].fd, buff);

                        char msg_type[4];

                        strncpy(msg_type, &buff[0], 3);
                        msg_type[3] = '\0';
                        if (!strcmp(msg_type, "ATT")) {
                            if (i == 0) {
                                h2--;
                            } else {
                                h1--;
                            }
                        }
                        char state[MAX_MSG_LEN];
                        sprintf(state, "P1: %d - P2: %d", h1, h2);
                        printf("%s\n", state);
                        send_clients_msg(pfds[0].fd, pfds[1].fd, state);
                        
                        if (h1 == 0) {
                            send_client_msg(pfds[1].fd, "YOU WON");
                            send_client_msg(pfds[0].fd, "YOU LOOSE");
                            gameover = 1;
                        }
                        if (h2 == 0) {
                            send_client_msg(pfds[0].fd, "YOU WON");
                            send_client_msg(pfds[1].fd, "YOU LOOSE");
                            gameover = 1;
                        }
                        
                        
                        bzero(buff, MAX_MSG_LEN);
					}
				} 
		} 
	}

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

	while((read_len = recv(socket, client_message, MAX_MSG_LEN, 0)) > 0) {

		client_message[read_len] = '\0';

        printf("\nClient %d: %s\n", socket, client_message);

		if (matching_request_handler(client_message, read_len, socket) == 1) {
            create_game_thread();
            break;
        }	

	}
    if (read_len <= 0) 
        printf("Player %d is disconnected.\n", socket);

	pthread_exit(NULL);
	return 0;
}

int matching_request_handler(char *message, int msg_len, int socket) {
    
    char msg_type[4];

    strncpy(msg_type, &message[0], 3);
    msg_type[3] = '\0';

   
    if (!strcmp(msg_type, "CCL")) {
        printf("Player cancel waiting opponent.\n");

        pthread_mutex_lock(&mutex_room_queue);
        int roomId = s_delete_node(roomQueue, socket);
        pthread_mutex_unlock(&mutex_room_queue);

        printf("Delete room: %d.\n", roomId);

        send_client_msg(socket, "OK");

        return 0;

    } else if (!strcmp(msg_type, "CRT")) {

        srand(time(NULL));   
        int room_id = rand() % 1000;  

        pthread_mutex_lock(&mutex_room_queue);
        s_enqueue(roomQueue, room_id, socket);
        pthread_mutex_unlock(&mutex_room_queue);

        printf("Created Room: %d for player %d\n", room_id, socket);

        send_client_msg(socket, "OK");

        return 0;

    } else if (!strcmp(msg_type, "JOI")) {

        char room_id_str[4];
        strncpy(room_id_str, &message[4], 3);
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
            printf("Match with room %d successfully.\n", room_id);
            send_clients_msg(client1, socket, "MAT");
        } else {
            printf("No available room with id: %d.\n", room_id);
            send_client_msg(socket, "FIL");
        }

        return 0;

    } else if (!strcmp(msg_type, "LIS")) {

        pthread_mutex_lock(&mutex_room_queue);
        struct Room* temp = roomQueue->front;

        if (temp == NULL) 
            printf("No room available.\n");

        while (temp != NULL) {
            printf("  Room: %d\n- Client1: %d - Client2: %d\n- Ready: %d - Start: %d.\n", temp->key, temp->client1, temp->client2, temp->is_ready, temp->is_start);
            temp = temp->next;
        }
        pthread_mutex_unlock(&mutex_room_queue);

        send_client_msg(socket, "OK");
        
        return 0;

    } else if (!strcmp(msg_type, "STA")) {

        send_client_msg(socket, "GAME START");

        return 1;

    } else {

        send_client_msg(socket, "INV");

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