import { Component, OnInit, output } from '@angular/core';
import { BaseComponent } from '../../base.component';

@Component({
  selector: 'app-connection-manager',
  templateUrl: './connection-manager.component.html',
  styles: [``],
  standalone: true,
  imports: []
})
export class ConnectionManagerComponent extends BaseComponent implements OnInit {

  closeForm = output<any>();

  constructor() {
    super();
  }

  ngOnInit() {
  }

}
