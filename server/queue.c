#include<stdio.h>
#include<stdlib.h>
#include "queue.h"
// A utility function to create a new linked list node.
struct Room* newNode(int roomId, int client_id)
{
    struct Room* temp = (struct Room*)malloc(sizeof(struct Room));
    temp->key = roomId;
    temp->is_ready = 0;
    temp->is_start = 0;
    temp->client1 = client_id;
    temp->prev = NULL;
    temp->next = NULL;
    return temp;
}
  
// A utility function to create an empty queue
struct S_Queue* s_createQueue()
{
    queue* q = (struct S_Queue*)malloc(sizeof(struct S_Queue));
    q->front = q->rear = NULL;
    return q;
}

  
// The function to add a key k to q
void s_enqueue(struct S_Queue* q, int roomId, int client_id)
{
    // Create a new LL node
    struct Room* temp = newNode(roomId, client_id);
  
    // If queue is empty, then new node is front and rear both
    if (q->rear == NULL) {
        q->front = q->rear = temp;
        return;
    }
  
    // Add the new node at the end of queue and change rear
    temp->prev = q->rear;
    q->rear->next = temp;    
    q->rear = temp;

}
  
// Function to remove a key from given queue q
int s_dequeue(struct S_Queue* q)
{
    // If queue is empty, return NULL.
    if (q->front == NULL)
        return 0;
    int val = q->front->key;
    // Store previous front and move front one node ahead
    struct Room* temp = q->front;
  
    q->front = q->front->next;
  
    // If front becomes NULL, then change rear also as NULL
    if (q->front == NULL)
        q->rear = NULL;
  
    free(temp);

    return val;
}

// Function to remove a key from given queue q
int s_delete_node(struct S_Queue* q, int client)
{   
    struct Room* temp = q->front;
    struct Room* prev = (struct Room*)malloc(sizeof(struct Room));
    prev = NULL;
    int roomId;
    while (temp != NULL) {
        // printf("Room: %d", temp->key);
        if (temp->client1 == client) {
            prev = temp;
            if (temp->prev != NULL) {
                temp->prev->next = temp->next;
            } else {
                q->front = temp->next;
            }
            if (temp->next != NULL) {
                temp->next->prev = temp->prev;
            } else {
                q->rear = temp->prev;
            }
            roomId = temp->key;
            free(temp);
            return roomId;
        }
        temp = temp->next;
    }

    return 0;
}

int isempty(queue* q){
    return(q->front == NULL && q->rear ==NULL);
}
// int main()
// {
//     struct Queue* q = createQueue();
//     enqueue(q, 10);
//     enqueue(q, 20);
//     dequeue(q);
//     dequeue(q);
//     enqueue(q, 30);
//     enqueue(q, 40);
//     enqueue(q, 50);
//     dequeue(q);
//     printf("Queue Front : %d \n", q->front->key);
//     printf("Queue Rear : %d", q->rear->key);
//     return 0;
// }