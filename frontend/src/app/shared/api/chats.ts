import { PageResult } from './page-result';

export interface MessageDto {
  id: string;
  senderId: string;
  senderName: string;
  text: string;
  sentAt: string;
  senderIsPrivileged: boolean;
}

export interface MutedUserDto {
  userId: string;
  displayName: string;
}

export interface ChatMessagesDto {
  messages: PageResult<MessageDto>;
  viewerIsPrivileged: boolean;
  viewerIsMuted: boolean;
  mutedUsers: MutedUserDto[];
}

export interface SendMessageDto {
  text: string;
}
