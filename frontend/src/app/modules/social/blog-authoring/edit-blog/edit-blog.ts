import { httpResource } from '@angular/common/http';
import { Component, effect, inject, input, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { ImagePicker } from '../../../../shared/images/image-picker/image-picker';
import { ImageUploading } from '../../../../shared/images/image-uploading';
import { serverMessage } from '../../../../shared/util/server-message';
import { BlogDto } from '../../api/social-api-types';
import { BlogAuthoring } from '../blog-authoring';

@Component({
  imports: [FormField, ImagePicker],
  selector: 'app-edit-blog',
  styleUrl: './edit-blog.scss',
  templateUrl: './edit-blog.html',
})
export class EditBlog {
  private readonly blogAuthoring = inject(BlogAuthoring);
  private readonly imageUploading = inject(ImageUploading);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected readonly blog = httpResource<BlogDto>(() => `/api/social/blogs/${this.id()}/edit`);

  protected readonly form = form(signal({ title: '', description: '' }), (path) => {
    required(path.title, { message: 'Title is required.' });
    required(path.description, { message: 'Description is required.' });
  });

  protected readonly images = signal<string[]>([]);

  protected readonly selected = signal<File | null>(null);
  protected readonly uploading = signal(false);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  private prefilled = false;

  constructor() {
    effect(() => {
      // Seed the editable form once the author's blog arrives from the server.
      const blog = this.blog.value();
      if (blog && !this.prefilled) {
        this.prefilled = true;
        this.form().reset({ title: blog.title, description: blog.description });
        this.images.set([...blog.images]);
      }
    });
  }

  protected pickImage(file: File | null): void {
    this.error.set(null);
    this.selected.set(file);
  }

  protected async uploadImage(): Promise<void> {
    const file = this.selected();
    if (file === null) {
      return;
    }
    this.uploading.set(true);
    this.error.set(null);
    try {
      const saved = await this.imageUploading.upload(file);
      this.images.update((list) => [...list, saved.address]);
      this.selected.set(null);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not upload the image.'));
    } finally {
      this.uploading.set(false);
    }
  }

  protected removeImage(index: number): void {
    this.images.update((list) => list.filter((_, position) => position !== index));
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.blogAuthoring.update(this.id(), {
        title: this.form.title().value(),
        description: this.form.description().value(),
        images: this.images(),
      });
      await this.router.navigate(['/social', this.id()]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the blog.'));
    } finally {
      this.pending.set(false);
    }
  }
}
