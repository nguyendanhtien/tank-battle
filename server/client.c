#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <string.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>
#include <pthread.h>
#include <signal.h>

#define MAX_MSG_LEN 64
void *send_msg_handler(void *sockfd);
void *recv_msg_handler(void *sockfd);
volatile sig_atomic_t flag = 0;


void send_msg(int sockfd, char* msg)
{
    int n = send(sockfd, msg, sizeof(msg), 0);;
    if (n < 0)
        printf("ERROR sending message to server socket");
    
    printf("[DEBUG] Sending message to server: %s\n", msg);
}

int connect_to_server(char * hostname, int portno)
{
    struct sockaddr_in serv_addr;
    struct hostent *server;
 
    int sockfd = socket(AF_INET, SOCK_STREAM, 0);
	
    if (sockfd < 0) 
        printf("ERROR opening socket for server.");
	
    server = gethostbyname(hostname);
	
    if (server == NULL) {
        fprintf(stderr,"ERROR, no such host\n");
        exit(0);
    }
	
	memset(&serv_addr, 0, sizeof(serv_addr));

   serv_addr.sin_family = AF_INET;
    memmove(server->h_addr, &serv_addr.sin_addr.s_addr, server->h_length);
    serv_addr.sin_port = htons(portno); 

   if (connect(sockfd, (struct sockaddr *) &serv_addr, sizeof(serv_addr)) < 0) 
        printf("ERROR connecting to server");

    printf("[DEBUG] Connected to server.\n");
     return sockfd;
}

int main(int argc, char *argv[]) {
    if (argc < 3) {
       fprintf(stderr,"usage %s hostname port\n", argv[0]);
       exit(0);
    }
    
    int sockfd = connect_to_server(argv[1], atoi(argv[2]));
   
    
    pthread_t send_msg_thread;
    pthread_t recv_msg_thread;
    int ret;
    ret = pthread_create(&send_msg_thread, NULL, send_msg_handler, &sockfd);
    if (ret) {
        printf("Thread creation failed with return code %d\n", ret);
        exit(-1);
    }

    ret = pthread_create(&recv_msg_thread, NULL, recv_msg_handler, &sockfd);
    if (ret) {
        printf("Thread creation failed with return code %d\n", ret);
        exit(-1);
    }

    
    // while((msg_len = recv(sockfd, &response, sizeof(response), 0)) > 0) {

    //     response[msg_len] = '\0';
    //     printf("Message from server: %s\n", response);
    //     memset(response, 0, MAX_MSG_LEN);
    //     printf("Enter message: ");
    //     scanf("%s",&response);
    //     send(sockfd, response, sizeof(response), 0);
    //     // if (strcmp(response,"exit")==0){break;}
    //         //print out the server's response
    // }
    // printf("Game over.\n");
    pthread_join(send_msg_thread,NULL);
    pthread_join(recv_msg_thread,NULL);
    close(sockfd);
    return 0;
}


void *send_msg_handler(void *sockfd) {
    int socket = *(int*) sockfd;
	int read_len;
    char msg[MAX_MSG_LEN];
	while (1) {
        // printf("Enter message: ");
        fgets(msg, MAX_MSG_LEN, stdin);
        send(socket, msg, strlen(msg), 0);
        if (!strcmp(msg, "exit"))
            break;
        bzero(msg, MAX_MSG_LEN);
    }
    pthread_exit(NULL);
	return 0;
}


void *recv_msg_handler(void *sockfd) {
    
    int socket = *(int*) sockfd;
	int msg_len;
    char msg[MAX_MSG_LEN];
    memset(msg, 0, MAX_MSG_LEN);
	while ((msg_len = recv(socket, msg, MAX_MSG_LEN, 0)) > 0) {
        char msg_type[5];
        msg[msg_len] = '\0';
        strncpy(msg_type, &msg[0], 4);
        msg_type[4] = '\0';
        if (!strcmp(msg_type, "MATC")) {
            char send_msg[MAX_MSG_LEN] = "PLAY";
            send(socket, send_msg, strlen(send_msg), 0);
            bzero(send_msg, MAX_MSG_LEN);
        }
        
        printf("\nFROM SERVER: %s\n", msg);
        fflush(stdout);

        bzero(msg, MAX_MSG_LEN);
    }
    pthread_exit(NULL);
	return 0;
}
