export interface UserGroup {
  userGroupId: number;
  groupName: string;
  description: string;
  isDeleted: boolean;
  createdAt: Date;
  updateAt: Date;
  listUser: string;
  treeViewPermissions: string;
}

export interface UserGroupDto {
  userGroupId?: number;
  groupName: string;
  description: string;
  isDeleted?: boolean;
  createdAt?: Date;
  updateAt?: Date;
  listUser: string;
  treeViewPermissions?: string;
}

export interface UserGroupCreateDto {
  groupName: string;
  description: string;
  listUser: string;
  treeViewPermissions?: string;
}

export interface UserGroupUpdateDto {
  userGroupId: number;
  groupName: string;
  description: string;
  listUser: string;
  treeViewPermissions?: string;
}

export interface TreeViewPermissionDto {
  userGroupId: number;
  treeViewPermissions: string;
}
