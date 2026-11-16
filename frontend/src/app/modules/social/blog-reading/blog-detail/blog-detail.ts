import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { marked } from 'marked';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogDto, VoteType } from '../../api/social-api-types';
import { BlogComments, CommentEdit, CommentError } from '../blog-comments/blog-comments';
import { BlogReading } from '../blog-reading';

@Component({
  imports: [BlogComments, RouterLink],
  selector: 'app-blog-detail',
  styleUrl: './blog-detail.scss',
  templateUrl: './blog-detail.html',
})
export class BlogDetail {
  private readonly blogReading = inject(BlogReading);
  protected readonly auth = inject(Auth);

  readonly id = input.required<string>();

  protected readonly detail = httpResource<BlogDto>(() => `/api/social/blogs/${this.id()}`);

  protected readonly descriptionHtml = computed(() => {
    const blog = this.detail.value();
    return blog ? (marked.parse(blog.description) as string) : '';
  });

  protected readonly scoreLabel = computed(() => {
    const blog = this.detail.value();
    if (!blog) {
      return '';
    }
    return Math.abs(blog.score) === 1 ? `${blog.score} point` : `${blog.score} points`;
  });

  // A failed vote shows under the vote buttons; a failed comment command shows in the comment
  // form or under the comment it was about. A successful command reloads the blog, which also
  // gives the comments fresh, empty forms.
  protected readonly error = signal<string | null>(null);
  protected readonly commentError = signal<CommentError | null>(null);

  protected async add(text: string): Promise<void> {
    this.clearErrors();
    try {
      await this.blogReading.addComment(this.id(), { text });
      this.detail.reload();
    } catch (failure) {
      this.commentError.set({
        commentId: null,
        message: serverMessage(failure, 'Could not add the comment.'),
      });
    }
  }

  protected async edit(edit: CommentEdit): Promise<void> {
    this.clearErrors();
    try {
      await this.blogReading.updateComment(this.id(), edit.commentId, { text: edit.text });
      this.detail.reload();
    } catch (failure) {
      this.commentError.set({
        commentId: edit.commentId,
        message: serverMessage(failure, 'Could not update the comment.'),
      });
    }
  }

  protected async remove(commentId: string): Promise<void> {
    this.clearErrors();
    try {
      await this.blogReading.deleteComment(this.id(), commentId);
      this.detail.reload();
    } catch (failure) {
      this.commentError.set({
        commentId,
        message: serverMessage(failure, 'Could not delete the comment.'),
      });
    }
  }

  protected async vote(type: VoteType): Promise<void> {
    this.clearErrors();
    try {
      await this.blogReading.castVote(this.id(), { type });
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not cast the vote.'));
    }
  }

  protected lastModified(iso: string): string {
    return new Date(iso).toLocaleString('sr-RS');
  }

  private clearErrors(): void {
    this.error.set(null);
    this.commentError.set(null);
  }
}
