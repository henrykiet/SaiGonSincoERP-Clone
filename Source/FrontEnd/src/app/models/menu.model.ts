export interface MenuDto {
  menuId: string;
  name: string;
  name2: string;
  parentMenuId: string;
  icon: string;
  url: string;
  voucherCode: string;
  type: string;
  hasAccess: boolean;
  hasInsert: boolean;
  hasUpdate: boolean;
  hasDel: boolean;
  children?: MenuDto[] | null; // Thay đổi thành optional và có thể null
  isExpanded?: boolean; // Thêm property cho UI
}

export interface MenuResponse {
  data: MenuDto[];
  success: boolean;
  message: string;
  statusCode: number;
} 