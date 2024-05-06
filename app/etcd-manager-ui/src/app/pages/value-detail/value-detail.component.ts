import { Component, OnInit, input } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { Dialog } from 'primeng/dialog';
import { BaseComponent } from '../../base.component';
import { CodeEditorConstant } from '../constants/code-editor.constant';
import { CodeEditorModule, CodeModel } from '@ngstack/code-editor';

@Component({
  selector: 'app-value-detail',
  templateUrl: './value-detail.component.html',
  styles: [``],
  standalone: true,
  imports: [CodeEditorModule]
})
export class ValueDetailComponent extends BaseComponent implements OnInit {
  dialog = input.required<Dialog>();
  value = input.required<string>();
  key = input<string>();

  public buttons: MenuItem[] = [];

  codeEditorConstant = CodeEditorConstant;
  codeModel!: CodeModel;

  ngOnInit() {
    this.codeModel = {
      ...CodeEditorConstant.DEFAULT_CODE_MODEL,
      value: this.value()
    }
  }

}
