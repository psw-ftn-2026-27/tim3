import { Routes } from '@angular/router';
import { CreateBlog } from './blog-authoring/create-blog/create-blog';
import { EditBlog } from './blog-authoring/edit-blog/edit-blog';
import { MyBlogs } from './blog-authoring/my-blogs/my-blogs';
import { BlogDetail } from './blog-reading/blog-detail/blog-detail';
import { BlogList } from './blog-reading/blog-list/blog-list';
import { ClubList } from './club-management/club-list/club-list';
import { CreateClub } from './club-management/create-club/create-club';
import { ClubDetail } from './club-membership/club-detail/club-detail';
import { MyInvitations } from './club-roster/my-invitations/my-invitations';
import { Profile } from './follow-management/profile/profile';
import { UserSearch } from './follow-requesting/user-search/user-search';

export const socialRoutes: Routes = [
  { path: '', component: BlogList },
  { path: 'mine', component: MyBlogs },
  { path: 'create', component: CreateBlog },
  { path: 'clubs', component: ClubList },
  { path: 'clubs/create', component: CreateClub },
  // Before 'clubs/:id', or "invitations" would be read as a club ID.
  { path: 'clubs/invitations', component: MyInvitations },
  { path: 'clubs/:id', component: ClubDetail },
  { path: 'users', component: UserSearch },
  { path: 'profile', component: Profile },
  { path: ':id/edit', component: EditBlog },
  { path: ':id', component: BlogDetail },
];
