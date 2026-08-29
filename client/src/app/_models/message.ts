export interface Message {
  id: number;
  content: string;
  senderUserId: number;
  recipientUserId: number;
  senderUsername: string;
  recipientUsername: string;
  createdAt: string;
}
