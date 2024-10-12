import { Component, ViewChild, effect, inject, model, signal, untracked } from '@angular/core';
import { ConfirmationService, MenuItem, Message, MessageService, PrimeIcons, TreeNode } from 'primeng/api';
import { ContextMenu, ContextMenuModule } from 'primeng/contextmenu';
import { DialogModule } from 'primeng/dialog';
import { FileUpload, FileUploadModule } from 'primeng/fileupload';
import { Listbox, ListboxModule } from 'primeng/listbox';
import { ToolbarModule } from 'primeng/toolbar';
import { Tree, TreeModule } from 'primeng/tree';
import { BaseComponent } from '../../../base.component';
import { commonLayoutImport } from '../../../layout/common-layout-import';
import { ImportNodesComponent } from '../../import-nodes/import-nodes.component';
import { ExportService } from '../../service/export.service';
import { KeyValueService } from '../../service/key-value.service';
import { LocalCacheService } from '../../service/local-cache.service';

@Component({
  selector: 'app-key-list',
  templateUrl: './key-list.component.html',
  styles: [`
  .key-list-item {
      width: 100%;

      i {
          margin-right: 10px;
      }
  }

  .button-header {
      &:hover {
          cursor: pointer;
      }

    margin-right:20px;
  }

  `],
  standalone: true,
  imports: [...commonLayoutImport, ContextMenuModule, ToolbarModule, ListboxModule, TreeModule,
    DialogModule, FileUploadModule, ImportNodesComponent
  ],

})
export class KeyListComponent extends BaseComponent {

  loaded = signal(false);
  viewMode: string = 'tree';
  showImportNodes = model(false);
  currentSelectRow?: any;
  contextMenuSelectedKey?: string;
  treeIsExpandAll = false;
  contextMenuModel: MenuItem[] = this.getContextMenu();
  msgs: Message[] = [];
  listSelectedItem = model<any>(null);
  treeSelectedItem = model<any>(null);
  firstLoad = true;
  primeIcons = PrimeIcons;
  @ViewChild('mainTree', { static: false }) mainTree?: Tree;
  @ViewChild('fileControl') fileControl!: FileUpload;
  @ViewChild('mainList', { static: false }) mainList!: Listbox;

  private readonly _messageService = inject(MessageService);
  private readonly _keyValueService = inject(KeyValueService);
  private readonly _localCacheService = inject(LocalCacheService);
  private readonly _confirmationService = inject(ConfirmationService);
  private readonly _exportService = inject(ExportService);

  constructor() {
    super();
    this.handleOnSelectEtcdConnection();
    this.handleOnNewKeySucceeded();
    this.handleOnRenameKeySucceeded();
    this.handleOneDeleteKeySucceeded();
    this.handleOnRequestFormNewKey();
  }

  private handleOnRequestFormNewKey() {
    effect(() => {
      if (this.globalStore.keyValues.isNewState()) {
        this.treeSelectedItem.update(() => null);
      }
    });
  }

  private handleOneDeleteKeySucceeded() {
    effect(() => {
      if (this.globalStore.keyValues.deleteSuccessAt()) {
        untracked(() => {
          this.refreshList(false, false);
        });
      }
    });
  }

  private handleOnRenameKeySucceeded() {
    effect(() => {
      if (this.globalStore.keyValues.renameKeySuccessAt()) {
        untracked(() => {
          this.refreshList();
        });
      }
    });
  }

  private handleOnNewKeySucceeded() {
    effect(() => {
      if (this.globalStore.keyValues.newKeySuccessAt()) {
        untracked(() => {
          this.refreshList();
        });
      }
    });
  }

  private handleOnSelectEtcdConnection() {
    effect(() => {
      const selectedConnection = this.globalStore.connections.selectedEtcdConnection();
      if (selectedConnection && selectedConnection.id > 0) {
        let selectedKey: string | null = null;
        if (this.firstLoad) {
          selectedKey = this._localCacheService.get('selectedKey');
          const viewMode = this._localCacheService.get('viewMode') || 'tree';
          this.switchViewMode(viewMode);
          this.firstLoad = false;
        }
        this.bindDataAndSelectExistItem(selectedConnection, selectedKey);
      }
    });
  }

  private bindDataAndSelectExistItem(selectedConnection: { id: number; name: string; }, selectedKey: string | null) {
    return new Promise(resolve => {
      this.bindData(selectedConnection.id, selectedKey).then(rs => {
        if (this.treeSelectedItem() != null && this.viewMode == 'tree') {
          this.onNodeSelect({ node: this.treeSelectedItem() });
        } else if (this.listSelectedItem() != null && this.viewMode == 'list') {
          this.onNodeSelect({ node: { data: this.listSelectedItem().key } });
        }
        resolve(rs);
      }).catch(err => {
        console.error(err);
        resolve(false);
      });
    });
  }

  getContextMenu() {
    const menu = [];

    if (this.viewMode == 'tree') {
      menu.push({
        label: 'Create child node',
        icon: 'pi pi-plus',
        command: this.createChildNode.bind(this)
      });
    }

    menu.push(...[
      {
        label: 'Delete',
        icon: 'pi pi-trash',
        command: this.menuDelete.bind(this)
      },]);

    if (this.currentSelectRow) {
      menu.push(
        {
          label: 'Export node',
          icon: 'pi pi-download',
          command: this.exportSelectedNode.bind(this)
        },
      );
    }

    if (this.viewMode == 'tree') {
      menu.push({
        label: 'Export node and all children',
        icon: 'pi pi-download',
        command: this.exportAllNodeAndChildren.bind(this)
      });
    }

    return menu;
  }

  menuDelete() {
    if (this.currentSelectRow) {
      this._confirmationService.confirm({
        message: 'Are you sure that you want to delete this key?',
        accept: () => {
          this._keyValueService.deleteKey(this.globalStore.connections.selectedEtcdConnection.id(), this.currentSelectRow.key).then(rs => {
            this.refreshList(false, false);
          }).catch(err => {
            this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
          });
        }
      });
    }
  }

  exportAllNodeAndChildren() {
    this._keyValueService.getByKeyPrefix(this.globalStore.connections.selectedEtcdConnection.id(), this.contextMenuSelectedKey!).then(rs => {
      this._exportService.exportNodes(rs);
    });
  }

  exportSelectedNode() {
    this._keyValueService.getByKey(this.globalStore.connections.selectedEtcdConnection.id(), this.currentSelectRow.key).then(rs => {
      this._exportService.exportJsonNode(rs);
    }).catch(err => {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: err.message });
    });
  }

  createChildNode() {
    if (this.treeSelectedItem) {
      const newKey = this.treeSelectedItem().data + '/new_key';
      this.globalStore.setIsNewState(true);
      this.globalStore.setDefaultNewKey(newKey);
    }
  }

  menuRename() {
    console.log('rename');
  }

  async bindData(selectedConnectionId: number, selectedKey?: string | null) {
    try {
      const ds = await this._keyValueService.getAllKeys(selectedConnectionId);
      const dataSource = ds.map((x: any) => {
        return { data: x, key: x };
      });

      this.listSelectedItem.update(() => dataSource.find(x => x.key == selectedKey));
      this.globalStore.setDataSourceKeys(dataSource);
      this.bindDataSourceTree(dataSource, selectedKey);
      this.loaded.update(() => true);
    } catch (err: any) {
      console.error(err);
      this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
      this.msgs.push({ severity: 'error', summary: 'Error', detail: err.error.error });
    }
  }

  onChangeSelectedKey(evt: any) {
    console.log('select key', evt);
    this.onNodeSelect({ node: { data: evt.value.key } });
  }

  async refreshList(showMessage: boolean = false, selectExistItem: boolean = true) {
    this.globalStore.setTreeLoading(true);
    const id = this.globalStore.connections.selectedEtcdConnection.id();
    if (id > 0) {
      const selectedKey = selectExistItem ? this.globalStore.keyValues.selectedKey() : null;
      this.treeSelectedItem.update(() => null);
      await this.bindDataAndSelectExistItem(this.globalStore.connections.selectedEtcdConnection(), selectedKey);
      if (showMessage) {
        this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Refresh success' });
      }
      this.globalStore.setTreeLoading(false);
    }
  }

  switchViewMode(viewMode?: string | null) {
    const isListView = (this.viewMode = viewMode ?? (this.viewMode === 'tree' ? 'list' : 'tree')) === 'list';
    const selectedItem = isListView ? this.treeSelectedItem() : this.listSelectedItem();

    if (isListView && selectedItem) {
      const key = selectedItem.data;
      this.listSelectedItem.update(() => ({ key, data: key }));
      this.currentSelectRow = this.listSelectedItem();
    } else if (!isListView && selectedItem) {
      this.treeSelectedItem.update(() => selectedItem);
      this.currentSelectRow = this.treeSelectedItem();
    }

    this.contextMenuModel = this.getContextMenu();
    this._localCacheService.set('viewMode', this.viewMode);
  }

  bindDataSourceTree(dataSource: any[], selectedKey?: string | null) {
    const paths = dataSource.map(x => x.key);
    const root: TreeNode<string> = {};
    for (let path of paths) {
      let pathParts = path.split('/');
      let currentNode = root;
      currentNode = this.processDatasourcePathPart(pathParts, currentNode, selectedKey);
    }
    const treeDataSource = root.children || [];
    this.globalStore.setTreeDataSource(treeDataSource);
  }

  private processDatasourcePathPart(pathParts: any, currentNode: TreeNode<string>, selectedKey: string | null | undefined) {
    for (let part of pathParts) {
      if (part !== '') {
        if (!currentNode.children) {
          currentNode.children = [];
        }
        let childNode = currentNode.children.find(node => node.label === part);
        if (!childNode) {
          childNode = { label: part, data: pathParts.slice(0, pathParts.indexOf(part) + 1).join('/'), expanded: true, expandedIcon: PrimeIcons.FOLDER_OPEN, icon: PrimeIcons.FOLDER } as TreeNode;
          childNode.key = childNode.data;
          // pre-selected node if view mode is tree
          if (this.viewMode === 'tree' && selectedKey && selectedKey === childNode.data) {
            this.treeSelectedItem.update(() => childNode);
          }
          currentNode.children.push(childNode);
        }
        currentNode = childNode;
      }
    }
    return currentNode;
  }

  nomarlizePathCombine(path: string) {
    if (path.startsWith('//')) {
      path = path.substring(1);
    }
    return path;
  }

  export() {
    this._keyValueService.getAll(this.globalStore.connections.selectedEtcdConnection.id()).then(rs => {
      this._exportService.exportNodes(rs);
    }).catch(err => {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
    });
  }

  import() {
    if (this.fileControl.basicFileInput) {
      this.fileControl.basicFileInput.nativeElement.click();
    }
  }

  newKey() {
    this.globalStore.setIsNewState(true);
  }

  showContextMenuViewModeList(menu: ContextMenu, event: MouseEvent, item: any) {
    menu.hide();
    event.preventDefault();
    event.stopPropagation();
    setTimeout(() => {
      this.currentSelectRow = item;
      this.contextMenuSelectedKey = item.key;
      this.contextMenuModel = this.getContextMenu();
      menu.toggle(event);
    });
  }

  contextMenuViewModeTreeSelect(evt: any) {
    this.currentSelectRow = this.globalStore.keyValues.dataSource().find(x => x.key == evt.node.data);
    this.contextMenuSelectedKey = evt.node.data;
    this.contextMenuModel = this.getContextMenu();
  }

  preventMouseDown(event: any) {
    event.preventDefault();
    event.stopPropagation();
  }

  onNodeSelect(evt: any) {
    this.globalStore.setSelectedKey(evt.node.data);
    // save to cache
    this._localCacheService.set('selectedKey', evt.node.data);
  }

  toggleExpandTree(isExpand: boolean) {
    this.treeIsExpandAll = isExpand;
    this.globalStore.keyValues.treeDataSource().forEach(node => {
      this.expandRecursive(node, isExpand);
    });
  }

  handleSelectFile(evt: any) {
    // this._exportService.readDataFromFile(evt.currentFiles[0]).then(rs => {
    //   if (rs) {
    //     console.log('file content', rs);
    //   }
    // });
  }

  importNodes() {
    this.showImportNodes.update(() => true);
  }

  private expandRecursive(node: TreeNode, isExpand: boolean) {
    node.expanded = isExpand;
    if (node.children) {
      node.children.forEach(childNode => {
        this.expandRecursive(childNode, isExpand);
      });
    }
  }

  closeDialogImportNodes() {
    this.showImportNodes.update(() => false);
  }

  onCommandButton(button: MenuItem, evt: any) {
    if (button.command) {
      button.command(evt);
    }
  }

  importSuccess() {
    this.showImportNodes.update(() => false);
    this.refreshList(false, true);
  }
}
