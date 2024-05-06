import { Component, OnInit } from '@angular/core';
import { commonLayoutImport } from '../../layout/common-layout-import';
import { BaseComponent } from '../../base.component';

@Component({
  selector: 'app-change-my-password',
  templateUrl: './change-my-password.component.html',
  styles: [``],
  standalone: true,
  imports: [...commonLayoutImport]
})
export class ChangeMyPasswordComponent extends BaseComponent implements OnInit {
  ngOnInit() {
  }
}
