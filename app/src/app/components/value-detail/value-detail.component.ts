import { Component, Input, OnInit } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { Dialog } from 'primeng/dialog';
import { CodeEditorConstant } from 'src/app/constants/code-editor.constant';

@Component({
    selector: 'app-value-detail',
    templateUrl: './value-detail.component.html',
    styleUrls: ['./value-detail.component.scss']
})
export class ValueDetailComponent implements OnInit {

    @Input() dialog: Dialog;
    @Input() value: any;
    public buttons: MenuItem[] = [];

    codeEditorConstant = CodeEditorConstant;
    codeModel: any;
    constructor() { }

    ngOnInit() {
        this.codeModel = {
            ...CodeEditorConstant.DEFAULT_CODE_MODEL,
            value: this.value
        }
    }

}
