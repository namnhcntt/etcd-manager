import { AfterViewInit, Component, ElementRef, NgZone, OnDestroy, ViewChild, inject, input, output } from '@angular/core';
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
export class DiffTextComponent extends BaseComponent implements AfterViewInit, OnDestroy {
  dialog = input.required<Dialog>();
  left = input<any>({});
  right = input<any>({});
  revision = input<number>(0);

  onClose = output<any>();

  @ViewChild('containerElement', { static: true }) containerElement!: ElementRef;

  private _ngZone = inject(NgZone);
  private originalModel: any;
  private modifiedModel: any;
  private diffEditor: any;

  ngAfterViewInit(): void {
    this._ngZone.runOutsideAngular(() => {
      this.originalModel = monaco.editor.createModel(
        this.left().value,
        'yaml'
      );
      this.modifiedModel = monaco.editor.createModel(
        this.right().value,
        'yaml'
      );

      this.diffEditor = monaco.editor.createDiffEditor(this.containerElement.nativeElement, {
        readOnly: true,
        originalEditable: false,
        automaticLayout: true,
      });
      this.diffEditor.setModel({
        original: this.originalModel,
        modified: this.modifiedModel
      });
    });
  }

  ngOnDestroy(): void {
    this.diffEditor?.dispose();
    this.diffEditor = null;
    this.originalModel?.dispose();
    this.originalModel = null;
    this.modifiedModel?.dispose();
    this.modifiedModel = null;
  }

}
