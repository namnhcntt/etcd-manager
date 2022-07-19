import { Component, OnInit } from '@angular/core';
import { MenuItem, MessageService, PrimeIcons, TreeNode } from 'primeng/api';
import { ContextMenu } from 'primeng/contextmenu';
import { AppCtxService } from 'src/app/service/app-ctx.service';
import { AppEventService } from 'src/app/service/app-event.service';
import { ComCtxService } from 'src/app/service/com-ctx.service';
import { KeyValueService } from 'src/app/service/key-value.service';

@Component({
    selector: 'app-key-list',
    templateUrl: './key-list.component.html',
    styleUrls: ['./key-list.component.scss'],
    providers: [ComCtxService]
})
export class KeyListComponent implements OnInit {
    viewMode: 'tree' | 'list' = 'list';
    parentKeyOnNew = '';
    showNewKeyForm = false;
    selectedKey: string;
    loaded = false;
    treeSelectedItem: any = {};
    treeDataSource: TreeNode[] = [];
    listDataSource = [];
    rootCtx: ComCtxService;
    currentSelectRow: any;
    contextMenuViewModeList: MenuItem[] = [
        {
            label: 'Create child node',
            icon: 'pi pi-plus',
            command: this.createChildNode.bind(this)
        },
        {
            label: 'Export current node',
            icon: 'pi pi-download',
            command: this.exportCurrentNode.bind(this)
        },
        {
            label: 'Export node and all childs',
            icon: 'pi pi-download',
            command: this.exportAllNodeAndChilds.bind(this)
        },
        {
            label: 'Rename',
            icon: 'pi pi-pencil',
            command: this.menuRename.bind(this)
        },
        {
            label: 'Delete',
            icon: 'pi pi-trash',
            command: this.menuDelete.bind(this)
        },
    ];

    constructor(
        private _appEventService: AppEventService,
        private _keyValueService: KeyValueService,
        private _messageService: MessageService,
        private _appCtxService: AppCtxService,
    ) {
        this.rootCtx = this._appCtxService.getRootCtx();
    }

    ngOnInit() {
        this._appEventService.getSubscriptionConnection().subscribe(async rs => {
            console.log('selected connection', rs);
            if (rs) {
                this.rootCtx.data.connection = rs;
                this.bindData(rs);
            }
        });

        this.rootCtx.subscribe('keyDeleted', rs => {
            this.refreshList();
        });
        this.rootCtx.subscribe('keyRenamed', rs => {
            this.refreshList();
        });
    }

    menuDelete() {
        this._keyValueService.onDelete(this.currentSelectRow.key).then(rs => {
            if (rs) {
                this.refreshList();
            }
        });
    }

    exportAllNodeAndChilds() {
        console.log('export all node and childs');
    }

    exportCurrentNode() {
        console.log('export current node');
    }

    createChildNode() {
        this.parentKeyOnNew = this.currentSelectRow.key + '/';
        this.showNewKeyForm = true;
    }

    menuRename() {
        console.log('rename');
    }

    async bindData(connection: any) {
        const ds = await this._keyValueService.getAll(connection);
        if (ds.success) {
            console.log('all keys', ds.data);
            this.listDataSource = ds.data.map(x => {
                return { key: x };
            });
            if (this.viewMode == 'tree') {
                this.bindDataSourceTree();
            }
            this.loaded = true;
        } else {
            this._messageService.add({ severity: 'error', summary: 'Error', detail: ds.message });
        }
    }

    onChangeSelectedKey(evt) {
        this.rootCtx.dispatchEvent('changeSelectedKey', evt.value);
    }

    refreshList() {
        this.bindData(this.rootCtx.data.connection);
        this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Refresh success' });
    }

    switchViewMode() {
        if (this.viewMode == 'list') {
            // list
            this.bindDataSourceTree();
            this.viewMode = 'tree';
        } else {
            // tree
            this.viewMode = 'list';
        }
        console.log('switch view mode');
    }

    bindDataSourceTree() {
        let paths = this.listDataSource.map(x => x.key);
        let result: TreeNode[] = [];
        let level = { result };
        paths.forEach(path => {
            let pathCombine = '';
            path.split('/').reduce((r, name, i, a) => {
                pathCombine += '/' + name;
                if (!r[name]) {
                    r[name] = { label: name, data: this.nomarlizePathCombine(pathCombine), children: [], result: [] } as TreeNode;
                    if (pathCombine != '//') {
                        r.result.push({
                            label: name == '' ? '/' : name,
                            data: this.nomarlizePathCombine(pathCombine),
                            children: r[name].result,
                            expanded: true,
                            collapsedIcon: PrimeIcons.FOLDER,
                            expandedIcon: PrimeIcons.FOLDER_OPEN,
                        } as TreeNode);
                    }
                } else if (i === a.length - 1) {
                    r.result.push({ label: name, data: this.nomarlizePathCombine(pathCombine), children: [], expanded: true });
                }
                return r[name];
            }, level)
        })
        this.treeDataSource = result;
    }

    nomarlizePathCombine(path: string) {
        if (path.startsWith('//')) {
            path = path.substring(1);
        }
        return path;
    }

    export() {
        console.log('export');
    }

    import() {
        console.log('import')
    }

    newKey() {
        console.log('newkey');
        this.showNewKeyForm = true;
    }

    onSaveNewKey(evt) {
        this.bindData(this.rootCtx.data.connection);
        this.showNewKeyForm = false;
    }

    showContextMenuViewModeList(menu: ContextMenu, event: MouseEvent, item) {
        menu.hide();
        event.preventDefault();
        event.stopPropagation();
        setTimeout(() => {
            console.log('context menu', menu, event, item);
            this.currentSelectRow = item;
            menu.toggle(event);
        }, 1);
    }

    contextMenuViewModeTreeSelect(evt) {
        this.currentSelectRow = this.listDataSource.find(x => x.key == evt.node.data);
    }

    preventMouseDown(event) {
        event.preventDefault();
        event.stopPropagation();
    }

    onNodeSelect(evt) {
        this.selectedKey = this.treeSelectedItem.data;
        this.rootCtx.dispatchEvent('changeSelectedKey', { key: this.selectedKey });
    }
}
