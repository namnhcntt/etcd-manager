import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from 'src/environments/environment';
import { AppCtxService } from './app-ctx.service';
import { ComCtxService } from './com-ctx.service';

@Injectable({
    providedIn: 'root'
})
export class ExportService {

    readonly rootCtx: ComCtxService;
    constructor(
        private _httpClient: HttpClient,
        private _appCtxService: AppCtxService
    ) {
        this.rootCtx = this._appCtxService.getRootCtx();
    }

    exportNodes(nodes: string[]) {
        const json = JSON.stringify(nodes);
        const blob = new Blob([json], { type: 'application/json' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `nodes_${this.rootCtx.data.connection.name}.json`;
        link.click();
        window.URL.revokeObjectURL(url);
    }

    exportJsonNode(nodeItem: any) {
        const json = JSON.stringify(nodeItem, null, 2);
        const blob = new Blob([json], { type: 'application/json' });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${nodeItem.key}.json`;
        link.click();
        window.URL.revokeObjectURL(url);
    }

    readDataFromFile(file: File): Promise<any> {
        return new Promise((resolve) => {
            const reader = new FileReader();
            reader.readAsText(file);
            reader.onload = (e) => {
                const obj = JSON.parse(e.target.result as string);
                resolve(obj);
            };
            reader.onerror = (e) => {
                resolve(null);
            }
        });
    }
}
