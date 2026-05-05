// src/services/audit/audit-login.service.ts
import { API_BASE_URL } from '@/config/api.config';
import { authFetch } from '@/utils/fetcher';

const AUDIT_LOGIN_LIST_URL = `${API_BASE_URL}AuditLogin/GetListAuditLogin`;

export interface FetchAuditLoginParams {
  username?: string;
  isSuccess?: boolean;
  startDate?: string;   // format: YYYY-MM-DD
  endDate?: string;     // format: YYYY-MM-DD
  page: number;
  pageSize: number;
}

export async function fetchAuditLogins(
  params: FetchAuditLoginParams
) {
  const query = new URLSearchParams({
    username: params.username ?? '',
    isSuccess:
      params.isSuccess !== undefined
        ? params.isSuccess.toString()
        : '',
    startDate: params.startDate ?? '',
    endDate: params.endDate ?? '',
    page: params.page.toString(),
    pageSize: params.pageSize.toString(),
  }).toString();

  const res = await authFetch(`${AUDIT_LOGIN_LIST_URL}?${query}`);
  const json = await res.json();

  if (!res.ok) {
    throw new Error(json?.message || 'Gagal mengambil data audit login');
  }

  return json;
}
