import { NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { BadgeModule } from 'primeng/badge';
import { InputSwitchModule } from 'primeng/inputswitch';
import { InputTextModule } from 'primeng/inputtext';
import { RadioButtonModule } from 'primeng/radiobutton';
import { RippleModule } from 'primeng/ripple';
import { SidebarModule } from 'primeng/sidebar';

export const commonLayoutImport = [
  FormsModule,
  HttpClientModule,
  InputTextModule,
  SidebarModule,
  BadgeModule,
  RadioButtonModule,
  InputSwitchModule,
  RippleModule,
  RouterModule,
  NgClass,
  NgIf,
  NgFor,
  NgStyle
];
