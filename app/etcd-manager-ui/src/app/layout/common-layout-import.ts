import { NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { BadgeModule } from 'primeng/badge';
import { ButtonModule } from 'primeng/button';
import { RippleModule } from 'primeng/ripple';

export const commonLayoutImport = [
  FormsModule,
  ReactiveFormsModule,
  HttpClientModule,
  BadgeModule,
  ButtonModule,
  RippleModule,
  RouterModule,
  NgClass,
  NgIf,
  NgFor,
  NgStyle
];
