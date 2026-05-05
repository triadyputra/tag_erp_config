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
          // 🔑 NORMALISASI DATA DI SINI
          const mapped: ComboItem[] = json.map((x: any) => ({
            value: x.Id,
            title: x.Name,
          }));

          setCabang(json);
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
