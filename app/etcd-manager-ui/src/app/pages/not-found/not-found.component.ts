import { Component, OnInit } from '@angular/core';
import { BaseComponent } from '../../base.component';
import { commonLayoutImport } from '../../layout/common-layout-import';

@Component({
  selector: 'app-not-found',
  templateUrl: './not-found.component.html',
  styles: [``],
  standalone: true,
  imports: [...commonLayoutImport],

})
export class NotFoundComponent extends BaseComponent implements OnInit {

  ngOnInit() {
  }

}
