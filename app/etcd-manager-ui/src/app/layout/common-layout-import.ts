import { JsonPipe, NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterLink, RouterModule } from '@angular/router';
import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';
import { RippleModule } from 'primeng/ripple';

export const commonLayoutImport = [
  FormsModule,
  ReactiveFormsModule,
  BadgeModule,
  ButtonModule,
  RippleModule,
  RouterModule,
  NgClass,
  NgIf,
  NgFor,
  NgStyle,
  RouterLink,
  JsonPipe
];
