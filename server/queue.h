#ifndef  _QUEUE_H_
#define _QUEUE_H_

struct Room {
    int key;
    int is_matched;
    int is_start;
    int is_client1_ready;
    int is_client2_ready;
    int client1;
    int client2;
    struct Room* prev;
    struct Room* next;
};
  
// The queue, front stores the front node of LL and rear stores the
// last node of LL
struct S_Queue {
    struct Room *front, *rear;
};
typedef struct S_Queue queue;
  
// A utility function to create a new linked list node.
struct Room* newNode(int k, int client_id);

// A utility function to create an empty queue
struct S_Queue* s_createQueue();

  
// The function to add a key k to q
void s_enqueue(struct S_Queue* q, int k, int client_id);

int s_delete_node(struct S_Queue* q, int client);

int s_delete_node_by_room_id(struct S_Queue* q, int room_id);

// Function to remove a key from given queue q
int s_dequeue(struct S_Queue* q);

//check empty queue
int isempty(queue* q);
#endif 