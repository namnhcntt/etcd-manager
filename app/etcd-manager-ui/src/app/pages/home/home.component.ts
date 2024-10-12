import { Component, OnInit } from '@angular/core';
import { SplitterModule } from 'primeng/splitter';
import { BaseComponent } from '../../base.component';
import { commonLayoutImport } from '../../layout/common-layout-import';
import { KeyDetailComponent } from './key-detail/key-detail.component';
import { KeyListComponent } from './key-list/key-list.component';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styles: [`
    .splitter-home {
    display: flex;
}

.app-key-list {
    width: 100%;
    align-self: stretch;
}

.app-key-detail {
    width: 100%;
}
  `],
  standalone: true,
  imports: [...commonLayoutImport, SplitterModule, KeyListComponent, KeyDetailComponent],

})
export class HomeComponent extends BaseComponent implements OnInit {

  ngOnInit() {
  }

}
