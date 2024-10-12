import { ChangeDetectionStrategy, Component, OnInit, output } from '@angular/core';
import { BaseComponent } from '../../base.component';

@Component({
  selector: 'app-user-manager',
  templateUrl: './user-manager.component.html',
  styles: [``],
  standalone: true,
  imports: [],

})
export class UserManagerComponent extends BaseComponent implements OnInit {
  closeForm = output<any>();
  ngOnInit() {
  }

}
