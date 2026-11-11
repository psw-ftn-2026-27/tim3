import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../auth/auth';

@Component({
  imports: [RouterLink],
  selector: 'app-home',
  styleUrl: './home.scss',
  templateUrl: './home.html',
})
export class Home {
  protected readonly auth = inject(Auth);
}
