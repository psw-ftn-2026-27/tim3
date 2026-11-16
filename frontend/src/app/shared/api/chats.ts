export interface MessageDto {
  id: string;
  senderId: string;
  senderName: string;
  text: string;
  sentAt: string;
}

export interface SendMessageDto {
  text: string;
}
