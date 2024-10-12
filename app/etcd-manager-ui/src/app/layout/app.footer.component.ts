import { Component, inject } from '@angular/core';
import { commonLayoutImport } from './common-layout-import';
import { LayoutService } from "./service/app.layout.service";

@Component({
  selector: 'app-footer',
  templateUrl: './app.footer.component.html',
  standalone: true,
  imports: [...commonLayoutImport],

})
export class AppFooterComponent {
  public layoutService = inject(LayoutService);
}
