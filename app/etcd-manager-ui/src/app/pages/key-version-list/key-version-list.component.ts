import { ChangeDetectionStrategy, Component } from '@angular/core';
import { BaseComponent } from '../../base.component';
import { commonLayoutImport } from '../../layout/common-layout-import';
import { DialogModule } from 'primeng/dialog';
import { DiffTextComponent } from '../diff-text/diff-text.component';

@Component({
  selector: 'app-key-version-list',
  templateUrl: './key-version-list.component.html',
  styles: [``],
  standalone: true,
  imports: [...commonLayoutImport, DialogModule, DiffTextComponent],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class KeyVersionListComponent extends BaseComponent {


}
