import { AfterViewInit, Component, ElementRef, EventEmitter, Input, NgZone, OnInit, Output, ViewChild } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { Dialog } from 'primeng/dialog';

declare var monaco: any;

@Component({
    selector: 'app-diff-text',
    templateUrl: './diff-text.component.html',
    styleUrls: ['./diff-text.component.scss']
})
export class DiffTextComponent implements OnInit, AfterViewInit {

    @Input() dialog: Dialog;
    @Input() key: string;
    @Input() left: any = {};
    @Input() right: any = {};
    @Input() revision = 0;

    @Output() onClose = new EventEmitter<any>();

    @ViewChild('containerElement', { static: true }) containerElement: ElementRef;

    public buttons: MenuItem[] = [];
    constructor(private _ngZone: NgZone) { }
    ngOnInit() {
        this.buttons = [
            {
                label: 'Cancel', icon: 'pi pi-times', command: (event) => {
                    this.dialog.close({} as Event);
                },
                styleClass: 'p-ripple p-button-raised p-button-secondary p-button-text'
            }
        ]
    }

    ngAfterViewInit(): void {
        this._ngZone.runOutsideAngular(() => {
            const originalModel = monaco.editor.createModel(
                this.left.value,
                'yaml'
            );
            const modifiedModel = monaco.editor.createModel(
                this.right.value,
                'yaml'
            );

            console.log('container', document.getElementById('container'));
            const diffEditor = monaco.editor.createDiffEditor(document.getElementById('container'));
            diffEditor.setModel({
                original: originalModel,
                modified: modifiedModel
            });

            const navi = monaco.editor.createDiffNavigator(diffEditor, {
                followsCaret: true, // resets the navigator state when the user selects something in the editor
                ignoreCharChanges: true // jump from line to line
            });

            // window.setInterval(function () {
            //     navi.next();
            // }, 2000);
        });
    }
}
