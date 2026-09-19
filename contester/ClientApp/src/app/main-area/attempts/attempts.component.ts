import { Component } from '@angular/core';
import { ProblemAttemptsComponent } from '../problem/problem-attempts/problem-attempts.component';

@Component({
  selector: 'app-attempts',
  standalone: true,
  imports: [
    ProblemAttemptsComponent,
  ],
  templateUrl: './attempts.component.html',
  styleUrl: './attempts.component.css'
})
export class AttemptsComponent {}
