import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { LayoutService } from "./service/app.layout.service";
import { commonLayoutImport } from './common-layout-import';

@Component({
  selector: 'app-footer',
  templateUrl: './app.footer.component.html',
  standalone: true,
  imports: [...commonLayoutImport],

})
export class AppFooterComponent {
  public layoutService = inject(LayoutService);
}
