"use client";

import { authFetch } from "@/utils/fetcher";
import { useEffect, useState } from "react";

interface ComboItem {
  value: string;
  title: string;
}

const BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL

export function useComboGroup() {
  const [groups, setGroups] = useState<ComboItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let active = true;

    async function fetchGroup() {
      try {
        const res = await authFetch(
          `${BASE_URL}Combo/ComboGroup`
        );
        const json = await res.json();
        console.log(json)
        if (active && json) {
          setGroups(json);
        }
      } catch (err) {
        console.error("Gagal load combo group", err);
      } finally {
        if (active) setLoading(false);
      }
    }

    fetchGroup();

    return () => {
      active = false;
    };
  }, []);

  return { groups, loading };
}


export function useComboCabang() {
  const [cabang, setCabang] = useState<ComboItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let active = true;

    async function fetchCabang() {
      try {
        const res = await authFetch(`${BASE_URL}Combo/ComboCabangTag`);
        const json = await res.json();

        if (active && Array.isArray(json)) {
          const mapped: ComboItem[] = json.map((x: any) => ({
            value: String(x.value ?? x.KdCabang ?? x.Id ?? ""),
            title: String(x.title ?? x.NmCabang ?? x.Name ?? ""),
          }));

          setCabang(mapped);
        }
      } catch (err) {
        console.error("Gagal load combo cabang", err);
      } finally {
        if (active) setLoading(false);
      }
    }

    fetchCabang();

    return () => {
      active = false;
    };
  }, []);

  return { cabang, loading };
}

export interface MasterKtpOption {
  NOKTP: string;
  NAMALENGKAP: string;
  KELAMIN?: string;
  KDCABANG?: string;
}

function parseComboApiJson(json: any) {
  const metadata = json?.Metadata ?? json?.metadata;
  if (metadata?.Success === false || (metadata?.Code && metadata.Code !== "200")) {
    throw new Error(metadata?.Message || metadata?.message || "Request gagal");
  }
  return json?.Data ?? json?.data ?? json;
}

export async function getFilterMasterKtp(nama?: string, cabang?: string) {
  const params = new URLSearchParams();
  if (nama) params.append("nama", nama);
  if (cabang) params.append("cabang", cabang);

  const res = await authFetch(
    `${BASE_URL}Combo/GetFilterMasterKtp?${params.toString()}`
  );
  const json = await res.json();
  if (!res.ok) {
    throw new Error(json?.Metadata?.Message || "Gagal mengambil data KTP");
  }
  return parseComboApiJson(json) as MasterKtpOption[];
}

export async function getDetailMasterKtp(noktp: string) {
  const res = await authFetch(
    `${BASE_URL}Combo/GetDetailMasterKtp?noktp=${encodeURIComponent(noktp)}`
  );
  const json = await res.json();
  if (!res.ok) {
    throw new Error(json?.Metadata?.Message || "Gagal mengambil detail KTP");
  }
  return parseComboApiJson(json) as MasterKtpOption;
}
