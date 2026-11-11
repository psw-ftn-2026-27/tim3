export type BlogStatus = 'Draft' | 'Published' | 'Closed';

export interface CommentDto {
  id: string;
  userId: string;
  text: string;
  createdAt: string;
}

export interface BlogDto {
  id: string;
  authorId: string;
  title: string;
  description: string;
  createdAt: string;
  lastModifiedAt: string;
  images: string[];
  status: BlogStatus;
  comments: CommentDto[];
}

export interface CreateBlogDto {
  title: string;
  description: string;
  images: string[] | null;
}

export interface UpdateBlogDto {
  title: string;
  description: string;
  images: string[] | null;
}

export interface CreateCommentDto {
  text: string;
}

export interface UpdateCommentDto {
  text: string;
}

export interface ClubDto {
  id: string;
  ownerId: string;
  name: string;
  description: string;
  images: string[];
}

export interface CreateClubDto {
  name: string;
  description: string;
  images: string[];
}

export interface UpdateClubDto {
  name: string;
  description: string;
  images: string[];
}

export type RelationKind = 'CanFollow' | 'Pending' | 'Following' | 'BlockedUntil' | 'Self';

export type FollowRequestStatus = 'Pending' | 'Accepted' | 'Rejected';

export interface RelationDto {
  kind: RelationKind;
  requestId: string | null;
  availableFrom: string | null;
}

export interface FoundUserDto {
  userId: string;
  displayName: string;
  relation: RelationDto;
}

export interface SentFollowRequestDto {
  id: string;
  recipientId: string;
  recipientDisplayName: string;
  sentAt: string;
  status: FollowRequestStatus;
}

export interface IncomingFollowRequestDto {
  id: string;
  senderId: string;
  senderDisplayName: string;
  sentAt: string;
}

export interface SendFollowRequestDto {
  recipientId: string;
}

export interface ProfileSummaryDto {
  followerCount: number;
  followingCount: number;
}

export interface ConnectedUserDto {
  userId: string;
  displayName: string;
}
