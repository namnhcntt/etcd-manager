import { Component, ElementRef, NgZone, ViewChild, inject, input, output } from '@angular/core';
import { Dialog, DialogModule } from 'primeng/dialog';
import { BaseComponent } from '../../base.component';
import { commonLayoutImport } from '../../layout/common-layout-import';

declare var monaco: any;

@Component({
  selector: 'app-diff-text',
  templateUrl: './diff-text.component.html',
  styles: [``],
  standalone: true,
  imports: [...commonLayoutImport, DialogModule],

})
export class DiffTextComponent extends BaseComponent {
  dialog = input.required<Dialog>();
  left = input<any>({});
  right = input<any>({});
  revision = input<number>(0);

  onClose = output<any>();

  @ViewChild('containerElement', { static: true }) containerElement!: ElementRef;

  private _ngZone = inject(NgZone);

  ngAfterViewInit(): void {
    this._ngZone.runOutsideAngular(() => {
      const originalModel = monaco.editor.createModel(
        this.left().value,
        'yaml'
      );
      const modifiedModel = monaco.editor.createModel(
        this.right().value,
        'yaml'
      );

      const diffEditor = monaco.editor.createDiffEditor(document.getElementById('container'), {
        readOnly: true,
        originalEditable: false,
        automaticLayout: true,
      });
      diffEditor.setModel({
        original: originalModel,
        modified: modifiedModel
      });
    });
  }

}
