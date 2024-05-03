import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { commonLayoutImport } from '../../layout/common-layout-import';
import { BaseComponent } from '../../base.component';

@Component({
  selector: 'app-not-found',
  templateUrl: './not-found.component.html',
  styles: [``],
  standalone: true,
  imports: [...commonLayoutImport],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotFoundComponent extends BaseComponent implements OnInit {

  ngOnInit() {
  }

}
