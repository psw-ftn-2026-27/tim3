import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BlogDto } from '../../api/social-api-types';

@Component({
  imports: [RouterLink],
  selector: 'app-blog-card',
  styleUrl: './blog-card.scss',
  templateUrl: './blog-card.html',
})
export class BlogCard {
  readonly blog = input.required<BlogDto>();

  // "Nov 9, 2026"
  protected readonly published = computed(() =>
    new Date(this.blog().createdAt).toLocaleDateString('en-US', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
    }),
  );

  protected readonly commentCount = computed(() => {
    const count = this.blog().comments.length;
    return count === 1 ? '1 comment' : `${count} comments`;
  });
}
