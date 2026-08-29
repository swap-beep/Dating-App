export interface AppNotification {
  id: number;
  message: string;
  type: 'like' | 'match';
  isRead: boolean;
  createdAt: string;
}
