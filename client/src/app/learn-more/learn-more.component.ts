import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-learn-more',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './learn-more.component.html',
  styleUrl: './learn-more.component.css'
})
export class LearnMoreComponent {}
