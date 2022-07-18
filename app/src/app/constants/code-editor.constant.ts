import { CodeModel } from '@ngstack/code-editor';

export class CodeEditorConstant {
    static DEFAULT_CODE_MODEL: CodeModel = {
        language: 'yaml',
        // uri: '*.yaml',
        // value: ''
    } as CodeModel;

    static DEFAULT_THEME = 'vs-light';
    static DEFAULT_OPTIONS = {
        contextmenu: true,
        minimap: {
            enabled: true
        },
        lineNumbers: true,
    };
}
