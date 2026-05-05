// src/services/menu/menu.service.ts

import { API_BASE_URL } from '@/config/api.config';
import { parseApiResponse } from '@/helpers/global.helper';
import { authFetch } from '@/utils/fetcher';

// ===============================
// URL
// ===============================
const MENU_URL = `${API_BASE_URL}MenuManagement`;
const MENU_LIST_URL = `${API_BASE_URL}MenuManagement/GetListMenu`;

// ===============================
// TYPES
// ===============================
export interface FetchMenuParams {
  filter?: string;
  page: number;
  pageSize: number;
}

// ===============================
// GET LIST MENU
// ===============================
export async function fetchMenu(params: FetchMenuParams) {
  const query = new URLSearchParams({
    filter: params.filter ?? '',
    page: params.page.toString(),
    pageSize: params.pageSize.toString(),
  }).toString();

  const res = await authFetch(`${MENU_LIST_URL}?${query}`);
  return parseApiResponse(res);
}

// ===============================
// GET DETAIL MENU
// ===============================
export async function fetchMenuById(id: string) {
  const res = await authFetch(`${MENU_URL}/${id}`);
  return parseApiResponse(res);
}

// ===============================
// CREATE / UPDATE MENU
// ===============================
export async function saveMenu(payload: any, isEdit: boolean) {
  const url = isEdit
    ? `${MENU_URL}/${payload.IdMenu}`
    : MENU_URL;

  const method = isEdit ? 'PUT' : 'POST';

  const res = await authFetch(url, {
    method,
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });

  return parseApiResponse(res);
}

// ===============================
// DELETE MENU
// ===============================
export async function deleteMenu(id: string) {
  const res = await authFetch(`${MENU_URL}/${id}`, {
    method: 'DELETE',
  });

  return parseApiResponse(res);
}