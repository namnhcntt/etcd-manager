import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ConfirmationService, MessageService } from 'primeng/api';
import { firstValueFrom } from 'rxjs';
import { environment } from 'src/environments/environment';
import { ResponseModel } from '../models/response.model';
import { AppCtxService } from './app-ctx.service';
import { ComCtxService } from './com-ctx.service';

@Injectable({
    providedIn: 'root'
})
export class KeyValueService {

    readonly ENDPOINT_GET_KEY_BY_CONNECTION = 'keyvalue/getall';
    readonly ENDPOINT_GET_DETAIL_BY_KEY = 'keyvalue/get';
    readonly ENDPOINT_DELETE_BY_KEY = 'keyvalue/delete';
    readonly ENDPOINT_SAVE = 'keyvalue/save';
    readonly ENDPOINT_RENAME_KEY = 'keyvalue/renamekey';
    readonly rootCtx: ComCtxService;
    constructor(
        private _httpClient: HttpClient,
        private _confirmationService: ConfirmationService,
        private _messageService: MessageService,
        private _appCtxService: AppCtxService
    ) {
        this.rootCtx = this._appCtxService.getRootCtx();
    }

    getAll(connection: any): Promise<ResponseModel> {
        const url = `${environment.apiEndpoint}/${this.ENDPOINT_GET_KEY_BY_CONNECTION}`;
        return firstValueFrom(this._httpClient.post<any>(url, connection));
    }

    getByKey(connection: any, key: string): Promise<ResponseModel> {
        const url = `${environment.apiEndpoint}/${this.ENDPOINT_GET_DETAIL_BY_KEY}?key=${encodeURIComponent(key)}`;
        return firstValueFrom(this._httpClient.post<ResponseModel>(url, connection));
    }

    save(connection: any, keyModel: any, isInsert = false): Promise<ResponseModel> {
        const url = `${environment.apiEndpoint}/${this.ENDPOINT_SAVE}`;
        keyModel.isInsert = isInsert;
        return firstValueFrom(this._httpClient.post<ResponseModel>(url, { connection, ...keyModel }));
    }

    delete(key: string, deleteRecursive = false): Promise<ResponseModel> {
        const url = `${environment.apiEndpoint}/${this.ENDPOINT_DELETE_BY_KEY}?key=${encodeURIComponent(key)}&deleteRecursive=${deleteRecursive}`;
        return firstValueFrom(this._httpClient.post<ResponseModel>(url, this.rootCtx.data.connection));
    }

    onDelete(key: string, deleteRecursive = false): Promise<any> {
        return new Promise((resolve) => {
            this._confirmationService.confirm({
                message: 'Are you sure that you want to delete?',
                accept: () => {
                    this.delete(key, deleteRecursive).then(rs => {
                        if (rs.success) {
                            resolve(true);
                            this.rootCtx.dispatchEvent('keyDeleted', { key });
                            this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Delete success' });
                        } else {
                            this._messageService.add({ severity: 'error', summary: 'Error', detail: rs.message });
                            resolve(false);
                        }
                    }).catch(err => {
                        resolve(false);
                        this._messageService.add({ severity: 'error', summary: 'Error', detail: err.message });
                    });
                },
            });
        })
    }

    isRootKey(key: string): boolean {
        return key === '/';
    }

    renameKey(oldKey: string, newKey: string): Promise<ResponseModel> {
        const url = `${environment.apiEndpoint}/${this.ENDPOINT_RENAME_KEY}?oldKey=${encodeURIComponent(oldKey)}&newKey=${encodeURIComponent(newKey)}`;
        return firstValueFrom(this._httpClient.post<ResponseModel>(url, this.rootCtx.data.connection));
    }

    getRevisionOfKey(key: string): Promise<ResponseModel> {
        const url = `${environment.apiEndpoint}/keyvalue/getrevision?key=${encodeURIComponent(key)}`;
        return firstValueFrom(this._httpClient.post<ResponseModel>(url, this.rootCtx.data.connection));
    }
}
