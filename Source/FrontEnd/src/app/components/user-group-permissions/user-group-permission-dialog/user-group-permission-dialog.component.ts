import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { FlatTreeControl } from '@angular/cdk/tree';
import { MatTreeFlatDataSource, MatTreeFlattener, MatTreeModule } from '@angular/material/tree';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';

interface MenuPermissionDTO {
  id: string;
  name: string;
  parentMenuId: string;
  hasAccess: boolean;
  canInsert: boolean;
  canUpdate: boolean;
  canDelete: boolean;
  isExpanded: boolean;
  children: Set<MenuPermissionDTO>;
}

interface SaveGroupPermissionRequestDTO {
  userGroupId: number;
  permissions: any[];
}

interface FlatNode {
  expandable: boolean;
  name: string;
  level: number;
  id: string;
  parentMenuId: string;
  hasAccess: boolean;
  canInsert: boolean;
  canUpdate: boolean;
  canDelete: boolean;
}

@Component({
  selector: 'app-user-group-permission-dialog',
  templateUrl: './user-group-permission-dialog.component.html',
  styleUrls: ['./user-group-permission-dialog.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatTreeModule,
    MatCheckboxModule,
    MatButtonModule,
    MatIconModule
  ]
})
export class UserGroupPermissionDialogComponent implements OnInit {
  private transformer = (node: MenuPermissionDTO, level: number): FlatNode => {
    return {
      expandable: !!node.children && node.children.size > 0,
      name: node.name,
      level: level,
      id: node.id,
      parentMenuId: node.parentMenuId,
      hasAccess: node.hasAccess,
      canInsert: node.canInsert,
      canUpdate: node.canUpdate,
      canDelete: node.canDelete
    };
  };

  treeControl = new FlatTreeControl<FlatNode>(
    node => node.level,
    node => node.expandable
  );

  treeFlattener = new MatTreeFlattener<MenuPermissionDTO, FlatNode, FlatNode>(
    this.transformer,
    node => node.level,
    node => node.expandable,
    node => Array.from(node.children)
  );

  dataSource = new MatTreeFlatDataSource(this.treeControl, this.treeFlattener);
  private flatNodes: FlatNode[] = [];

  constructor(
    public dialogRef: MatDialogRef<UserGroupPermissionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { userGroupId: number, groupName: string },
    private http: HttpClient
  ) {}

  ngOnInit() {
    this.loadPermissions();
  }

  hasChild = (_: number, node: FlatNode) => node.expandable;

  loadPermissions() {
    this.http.get<MenuPermissionDTO[]>(`${environment.apiUrl}/api/MenuPermission/usergroup/${this.data.userGroupId}`)
      .subscribe({
        next: (permissions) => {
          // Convert array to Set for each node's children
          const convertToSet = (items: MenuPermissionDTO[]): MenuPermissionDTO[] => {
            return items.map(item => {
              const convertedItem = {
                ...item,
                children: new Set<MenuPermissionDTO>()
              };
              
              if (item.children && Array.isArray(item.children)) {
                convertedItem.children = new Set(convertToSet(item.children));
              }
              
              return convertedItem;
            });
          };

          const convertedData = convertToSet(permissions);
          this.dataSource.data = convertedData;
          this.flatNodes = this.treeControl.dataNodes;
          this.treeControl.expandAll();
        },
        error: (error) => {
          console.error('Lỗi khi tải quyền nhóm:', error);
        }
      });
  }

  private findParentNode(node: FlatNode): FlatNode | null {
    const parentId = node.parentMenuId;
    if (!parentId) return null;
    
    return this.flatNodes.find(n => n.id === parentId) || null;
  }

  private findChildNodes(node: FlatNode): FlatNode[] {
    return this.flatNodes.filter(n => n.parentMenuId === node.id);
  }

  private findSiblingNodes(node: FlatNode): FlatNode[] {
    if (!node.parentMenuId) return [];
    return this.flatNodes.filter(n => n.parentMenuId === node.parentMenuId && n.id !== node.id);
  }

  private areAllChildrenUnchecked(node: FlatNode): boolean {
    const childNodes = this.findChildNodes(node);
    return childNodes.length > 0 && childNodes.every(child => !child.hasAccess);
  }

  private hasAnyCheckedChild(node: FlatNode): boolean {
    const childNodes = this.findChildNodes(node);
    return childNodes.some(child => child.hasAccess);
  }

  private updateParentAccess(node: FlatNode) {
    const parentNode = this.findParentNode(node);
    if (!parentNode) return;

    // Kiểm tra tất cả node con của node cha
    const allChildren = this.findChildNodes(parentNode);
    const allChildrenUnchecked = allChildren.every(child => !child.hasAccess);
    const anyChildChecked = allChildren.some(child => child.hasAccess);

    // Cập nhật trạng thái của node cha
    parentNode.hasAccess = anyChildChecked;

    // Nếu node cha bị bỏ check, bỏ check tất cả quyền con
    if (!parentNode.hasAccess) {
      parentNode.canInsert = false;
      parentNode.canUpdate = false;
      parentNode.canDelete = false;
    }

    // Đệ quy kiểm tra node cha của node cha
    this.updateParentAccess(parentNode);
  }

  updateNodeAccess(node: FlatNode, checked: boolean) {
    node.hasAccess = checked;
    
    if (checked) {
      // Khi check node cha, check tất cả node con
      const childNodes = this.findChildNodes(node);
      childNodes.forEach(child => {
        this.updateNodeAccess(child, true);
      });

      // Nếu check node con, check node cha trực tiếp
      const parentNode = this.findParentNode(node);
      if (parentNode) {
        parentNode.hasAccess = true;
        this.updateParentAccess(parentNode);
      }
    } else {
      // Khi bỏ check node cha, bỏ check tất cả node con
      node.canInsert = false;
      node.canUpdate = false;
      node.canDelete = false;
      
      const childNodes = this.findChildNodes(node);
      childNodes.forEach(child => {
        this.updateNodeAccess(child, false);
      });

      // Kiểm tra và cập nhật trạng thái của node cha
      this.updateParentAccess(node);
    }
  }

  updateNodeInsert(node: FlatNode, checked: boolean) {
    node.canInsert = checked;
  }

  updateNodeUpdate(node: FlatNode, checked: boolean) {
    node.canUpdate = checked;
  }

  updateNodeDelete(node: FlatNode, checked: boolean) {
    node.canDelete = checked;
  }

  private flattenPermissionsFromNodes(): any[] {
    // Tạo flat list từ flatNodes với trạng thái hiện tại
    return this.flatNodes.map(node => ({
      id: node.id,
      name: node.name,
      parentMenuId: node.parentMenuId,
      hasAccess: node.hasAccess,
      canInsert: node.canInsert,
      canUpdate: node.canUpdate,
      canDelete: node.canDelete,
      isExpanded: false,
      children: []
    }));
  }

  onSave() {
    // Cập nhật flatNodes trước khi lưu
    this.flatNodes = this.treeControl.dataNodes;
    
    const flatPermissions = this.flattenPermissionsFromNodes();
    console.log('🚀 Flat permissions to save:', flatPermissions);
    
    const request: SaveGroupPermissionRequestDTO = {
      userGroupId: this.data.userGroupId,
      permissions: flatPermissions
    };
    console.log('📤 Request payload:', request);

    this.http.post(`${environment.apiUrl}/api/MenuPermission/SaveUserGroupPermissions`, request)
      .subscribe({
        next: (response) => {
          console.log('✅ Save successful:', response);
          this.dialogRef.close(true);
        },
        error: (error) => {
          console.error('❌ Save failed:', error);
          console.error('Error details:', error.error);
        }
      });
  }

  onCancel() {
    this.dialogRef.close();
  }
}
