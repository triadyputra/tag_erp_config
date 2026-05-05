"use client";

import React, { useEffect, useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Grid,
  Stack,
  Typography,
  CircularProgress,
  Divider,
} from "@mui/material";

import { useSnackbar } from "@/app/context/SnackbarContext";
import { saveCabang } from "@/services/master-data/cabang.service";
import DialogHeader from "@/app/components/DialogHeader/DialogHeader";

interface Cabang {
  KdCabang?: string;
  NmCabang?: string;
  Alamat?: string;
  Telepon?: string;
  Fax?: string;
  KdPos?: string;
  Kota?: string;
  KaCab?: string;
  Grup?: string;
  NoCab?: string;
  KodeCab?: string;
}

interface Props {
  cabangToEdit: Cabang | null;
  onClose: () => void;
  onSaved?: () => void;
}

const FormCabang = ({ cabangToEdit, onClose, onSaved }: Props) => {
  const isEdit = Boolean(cabangToEdit?.KdCabang);
  const { showSnackbar } = useSnackbar();

  const [values, setValues] = useState<Cabang>({
    KdCabang: "",
    NmCabang: "",
    Alamat: "",
    Telepon: "",
    Fax: "",
    KdPos: "",
    Kota: "",
    KaCab: "",
    Grup: "",
    NoCab: "",
    KodeCab: "",
  });

  const [loading, setLoading] = useState(false);

  // =========================
  // INIT EDIT
  // =========================
  useEffect(() => {
    if (cabangToEdit) {
      setValues({
        KdCabang: cabangToEdit.KdCabang || "",
        NmCabang: cabangToEdit.NmCabang || "",
        Alamat: cabangToEdit.Alamat || "",
        Telepon: cabangToEdit.Telepon || "",
        Fax: cabangToEdit.Fax || "",
        KdPos: cabangToEdit.KdPos || "",
        Kota: cabangToEdit.Kota || "",
        KaCab: cabangToEdit.KaCab || "",
        Grup: cabangToEdit.Grup || "",
        NoCab: cabangToEdit.NoCab || "",
        KodeCab: cabangToEdit.KodeCab || "",
      });
    }
  }, [cabangToEdit]);

  // =========================
  // HANDLE CHANGE
  // =========================
  const handleChange = (field: keyof Cabang, value: string) => {
    setValues((prev) => ({
      ...prev,
      [field]: value,
    }));
  };

  // =========================
  // VALIDASI
  // =========================
  const validate = () => {
    if (!values.KdCabang)
      return "Kode Cabang wajib diisi";

    if (!values.NmCabang)
      return "Nama Cabang wajib diisi";

    return null;
  };

  // =========================
  // SUBMIT
  // =========================
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const error = validate();
    if (error) {
      showSnackbar(error, "error");
      return;
    }

    try {
      setLoading(true);

      await saveCabang(values);

      showSnackbar(
        isEdit ? "Cabang berhasil diperbarui" : "Cabang berhasil ditambahkan",
        "success"
      );

      onSaved?.();
      onClose();
    } catch (err: any) {
      showSnackbar(err.message || "Gagal menyimpan cabang", "error");
    } finally {
      setLoading(false);
    }
  };

  // =========================
  // UI STYLE
  // =========================
  const inputStyle = {
    "& .MuiOutlinedInput-root": {
      height: 34,
      fontSize: 13,
    },
  };

  return (
    <Dialog open={true} onClose={onClose} maxWidth="md" fullWidth>
      {/* HEADER */}
      <DialogHeader
        title={isEdit ? "Edit Akun" : "Tambah Akun"}
        subtitle="Pengisian dan pengelolaan informasi user"
        statusLabel={isEdit ? "EDIT" : "CREATE"}
        statusColor={isEdit ? "info" : "warning"}
      />

      <Divider />
      
      {/* FORM */}
      <form onSubmit={handleSubmit}>
        <DialogContent>
          <Grid container spacing={2}>
            {/* KODE */}
            <Grid  size={{ xs: 12, md: 4 }}>
              <TextField
                label="Kode Cabang"
                fullWidth
                size="small"
                value={values.KdCabang}
                onChange={(e) => handleChange("KdCabang", e.target.value.toUpperCase())}
                disabled={isEdit}
                sx={inputStyle}
              />
            </Grid>

            {/* NAMA */}
            <Grid size={{ xs: 12, md: 8 }}>
              <TextField
                label="Nama Cabang"
                fullWidth
                size="small"
                value={values.NmCabang}
                onChange={(e) => handleChange("NmCabang", e.target.value)}
                sx={inputStyle}
              />
            </Grid>

            {/* ALAMAT */}
            <Grid size={{ xs: 12 }}>
              <TextField
                label="Alamat"
                fullWidth
                size="small"
                value={values.Alamat}
                onChange={(e) => handleChange("Alamat", e.target.value)}
                sx={inputStyle}
              />
            </Grid>

            {/* KOTA */}
            <Grid size={{ xs: 12, md: 4 }}>
              <TextField
                label="Kota"
                fullWidth
                size="small"
                value={values.Kota}
                onChange={(e) => handleChange("Kota", e.target.value)}
                sx={inputStyle}
              />
            </Grid>

            {/* TELEPON */}
            <Grid size={{ xs: 12, md: 4 }}>
              <TextField
                label="Telepon"
                fullWidth
                size="small"
                value={values.Telepon}
                onChange={(e) => handleChange("Telepon", e.target.value)}
                sx={inputStyle}
              />
            </Grid>

            {/* FAX */}
            <Grid size={{ xs: 12, md: 4 }}>
              <TextField
                label="Fax"
                fullWidth
                size="small"
                value={values.Fax}
                onChange={(e) => handleChange("Fax", e.target.value)}
                sx={inputStyle}
              />
            </Grid>

            {/* KODE POS */}
            <Grid size={{ xs: 12, md: 4 }}>
              <TextField
                label="Kode Pos"
                fullWidth
                size="small"
                value={values.KdPos}
                onChange={(e) => handleChange("KdPos", e.target.value)}
                sx={inputStyle}
              />
            </Grid>

            {/* KEPALA CABANG */}
            <Grid size={{ xs: 12, md: 4 }}>
              <TextField
                label="Kepala Cabang"
                fullWidth
                size="small"
                value={values.KaCab}
                onChange={(e) => handleChange("KaCab", e.target.value)}
                sx={inputStyle}
              />
            </Grid>

            {/* GRUP */}
            <Grid size={{ xs: 12, md: 4 }}>
              <TextField
                label="Grup"
                fullWidth
                size="small"
                value={values.Grup}
                onChange={(e) => handleChange("Grup", e.target.value)}
                sx={inputStyle}
              />
            </Grid>

            {/* NO CAB */}
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                label="No Cabang"
                fullWidth
                size="small"
                value={values.NoCab}
                onChange={(e) => handleChange("NoCab", e.target.value)}
                sx={inputStyle}
              />
            </Grid>

            {/* KODE CAB */}
            <Grid size={{ xs: 12, md: 6 }}>
              <TextField
                label="Kode Cab"
                fullWidth
                size="small"
                value={values.KodeCab}
                onChange={(e) => handleChange("KodeCab", e.target.value)}
                sx={inputStyle}
              />
            </Grid>
          </Grid>
        </DialogContent>

        {/* ACTION */}
        <DialogActions
          sx={{
            px: 3,
            py: 2,
            borderTop: (theme) => `1px solid ${theme.palette.divider}`,
            display: "flex",
            justifyContent: "space-between",
          }}
        >
          <Typography
            fontSize={12}
            sx={(theme) => ({
              display: "inline-flex",
              alignItems: "center",
              gap: 0.5,
              px: 1.5,
              py: 0.5,
              borderRadius: 1,
  
              backgroundColor:
                theme.palette.mode === "dark"
                  ? theme.palette.primary.dark
                  : theme.palette.primary.light,
  
              color:
                theme.palette.mode === "dark"
                  ? theme.palette.primary.contrastText
                  : theme.palette.primary.main,
  
              border: `1px solid ${
                theme.palette.mode === "dark"
                  ? theme.palette.primary.main
                  : theme.palette.primary.light
              }`,
  
              fontStyle: "italic",
            })}
          >
            💡 Master Cabang Management
          </Typography>

          <Stack direction="row" spacing={1}>
            <Button onClick={onClose}>Batal</Button>

            <Button
              type="submit"
              variant="contained"
              disabled={loading}
            >
              {loading ? (
                <CircularProgress size={18} color="inherit" />
              ) : isEdit ? (
                "Update"
              ) : (
                "Simpan"
              )}
            </Button>
          </Stack>
        </DialogActions>
      </form>
    </Dialog>
  );
};

export default FormCabang;