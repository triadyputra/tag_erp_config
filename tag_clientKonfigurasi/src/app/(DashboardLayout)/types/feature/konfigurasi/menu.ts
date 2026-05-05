export interface MenuDto {
  IdMenu?: string;
  NamaMenu: string;
  NoUrut: number;
  Controllers: ControllerDto[];
}

export interface ControllerDto {
  IdController?: string;
  NamaController: string;
  NoUrut: number;
  Actions: ActionDto[];
}

export interface ActionDto {
  IdAction?: string;
  NamaAction: string;
  NoUrut: number;
}