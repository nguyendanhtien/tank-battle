#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <string.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netdb.h>

#define MAX_MSG_LEN 64

void recv_msg(int sockfd, char* msg)
{
    int n = read(sockfd, msg, 3);
    
    if (n < 0 || n != 3)
     error("ERROR reading message from server socket.");

    printf("[DEBUG] Received message: %s\n", msg);
   }

int recv_int(int sockfd)
{
    int msg = 0;
    int n = read(sockfd, &msg, sizeof(int));
    
    if (n < 0 || n != sizeof(int)) 
        error("ERROR reading int from server socket");
    
    printf("[DEBUG] Received int: %d\n", msg);
    
    return msg;
}

void write_server_int(int sockfd, int msg)
{
    int n = write(sockfd, &msg, sizeof(int));
    if (n < 0)
        error("ERROR writing int to server socket");
    
    printf("[DEBUG] Wrote int to server: %d\n", msg);
}

void send_msg(int sockfd, char* msg)
{
    int n = send(sockfd, msg, sizeof(msg), 0);;
    if (n < 0)
        error("ERROR sending message to server socket");
    
    printf("[DEBUG] Sending message to server: %s\n", msg);
}

void error(const char *msg)
{
    perror(msg);
    printf("Either the server shut down or the other player disconnected.\nGame over.\n");
    
    exit(0);
}
int connect_to_server(char * hostname, int portno)
{
    struct sockaddr_in serv_addr;
    struct hostent *server;
 
    int sockfd = socket(AF_INET, SOCK_STREAM, 0);
	
    if (sockfd < 0) 
        error("ERROR opening socket for server.");
	
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
        error("ERROR connecting to server");

    printf("[DEBUG] Connected to server.\n");
     return sockfd;
}

int main(int argc, char *argv[]) {
    if (argc < 3) {
       fprintf(stderr,"usage %s hostname port\n", argv[0]);
       exit(0);
    }

    int sockfd = connect_to_server(argv[1], atoi(argv[2]));
   
    char response[MAX_MSG_LEN];

    while(1) {
        
    	int msg_len = recv(sockfd, &response, sizeof(response), 0);
        if (msg_len == 0) {
            printf("Server terminated.");
            exit(4);
        }
        response[msg_len] = '\0';
        printf("Message from server: %s\n", response);
        memset(response, 0, MAX_MSG_LEN);
        printf("Enter message: ");
        scanf("%s",&response);
        send(sockfd, response, sizeof(response), 0);
        // if (strcmp(response,"exit")==0){break;}
            //print out the server's response
    }
    printf("Game over.\n");
    close(sockfd);
    return 0;
}

