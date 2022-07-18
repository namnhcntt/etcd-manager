import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CodeModel } from '@ngstack/code-editor';
import { CodeEditorConstant } from 'src/app/constants/code-editor.constant';

@Component({
    selector: 'live-code-editor',
    templateUrl: './live-code-editor.component.html',
    styleUrls: ['./live-code-editor.component.scss']
})
export class LiveCodeEditorComponent implements OnInit {

    codeEditorConstant = CodeEditorConstant;
    @Input() codeModel: CodeModel;

    @Output() valueChanged = new EventEmitter<string>();

    constructor() { }

    ngOnInit() {
    }

    onCodeChangedEvent(evt) {
        this.valueChanged.emit(evt);
    }
}
