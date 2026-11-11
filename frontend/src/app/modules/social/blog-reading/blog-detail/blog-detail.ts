import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { marked } from 'marked';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogDto } from '../../api/social-api-types';
import { BlogComments, CommentEdit } from '../blog-comments/blog-comments';
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

  protected readonly error = signal<string | null>(null);

  protected async add(text: string): Promise<void> {
    this.error.set(null);
    try {
      await this.blogReading.addComment(this.id(), { text });
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not add the comment.'));
    }
  }

  protected async edit(edit: CommentEdit): Promise<void> {
    this.error.set(null);
    try {
      await this.blogReading.updateComment(this.id(), edit.commentId, { text: edit.text });
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not update the comment.'));
    }
  }

  protected async remove(commentId: string): Promise<void> {
    this.error.set(null);
    try {
      await this.blogReading.deleteComment(this.id(), commentId);
      this.detail.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the comment.'));
    }
  }

  protected lastModified(iso: string): string {
    return new Date(iso).toLocaleString('sr-RS');
  }
}
