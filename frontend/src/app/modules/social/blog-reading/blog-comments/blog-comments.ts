import { Component, input, output, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { CommentDto } from '../../api/social-api-types';

export interface CommentEdit {
  commentId: string;
  text: string;
}

@Component({
  imports: [FormField],
  selector: 'app-blog-comments',
  styleUrl: './blog-comments.scss',
  templateUrl: './blog-comments.html',
})
export class BlogComments {
  readonly comments = input.required<CommentDto[]>();
  readonly currentUserId = input<string | null>(null);

  readonly addComment = output<string>();
  readonly editComment = output<CommentEdit>();
  readonly deleteComment = output<string>();

  protected readonly form = form(signal({ text: '' }), (path) => {
    required(path.text, { message: 'A comment cannot be empty.' });
  });

  protected readonly editingCommentId = signal<string | null>(null);

  protected readonly editForm = form(signal({ text: '' }), (path) => {
    required(path.text, { message: 'A comment cannot be empty.' });
  });

  // "Nov 15, 2026, 21:42"
  protected when(createdAt: string): string {
    return new Date(createdAt).toLocaleString('en-US', {
      day: 'numeric',
      hour: '2-digit',
      hour12: false,
      minute: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  }

  // True when the comment was edited after it was created.
  protected wasEdited(comment: CommentDto): boolean {
    return new Date(comment.lastModifiedAt).getTime() !== new Date(comment.createdAt).getTime();
  }

  // UX only: Edit and Delete stay visible until the server's edit deadline; the server
  // enforces the rule regardless.
  protected withinEditWindow(comment: CommentDto): boolean {
    return new Date(comment.editableUntil).getTime() >= Date.now();
  }

  protected submitComment(event: Event): void {
    event.preventDefault();
    this.addComment.emit(this.form.text().value());
    this.form().reset({ text: '' });
  }

  protected startEdit(comment: CommentDto): void {
    this.editingCommentId.set(comment.id);
    this.editForm().reset({ text: comment.text });
  }

  protected saveEdit(event: Event, commentId: string): void {
    event.preventDefault();
    this.editComment.emit({ commentId, text: this.editForm.text().value() });
    this.editingCommentId.set(null);
  }
}
