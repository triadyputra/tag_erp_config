"use client";

import React, { useEffect, useState } from "react";
import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TablePagination,
  Stack,
  TextField,
  InputAdornment,
  Button,
  Chip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  CircularProgress,
  TableContainer,
  Paper,
  alpha,
  Tooltip,
} from "@mui/material";

import { IconSearch, IconTrash, IconEdit } from "@tabler/icons-react";
import CustomCheckbox from "@/app/components/forms/theme-elements/CustomCheckbox";
import AccessButton from "@/app/components/buttons/AccessButton";
import useSWR from "swr";
import { swrFetcher } from "@/utils/swrFetcher";
import { useSnackbar } from "@/app/context/SnackbarContext";
import { deleteCabang } from "@/services/master-data/cabang.service";
import FormCabang from "./FormCabang";


interface Cabang {
  KdCabang: string;
  NmCabang: string;
  Kota?: string;
  Telepon?: string;
  Grup?: string;
}

const BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
const API_URL = `${BASE_URL}MstCabang`;

const CabangListComponent = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [selectAll, setSelectAll] = useState(false);
  const [openDeleteDialog, setOpenDeleteDialog] = useState(false);
  const [editing, setEditing] = useState<Cabang | null>(null);

  const { showSnackbar } = useSnackbar();

  // =======================
  // SWR
  // =======================
  const url = `${API_URL}/GetListCabang?filter=${encodeURIComponent(
    searchTerm
  )}&page=${page}&pageSize=${pageSize}`;

  const { data, error, mutate, isLoading } = useSWR(url, swrFetcher, {
    revalidateOnFocus: true,
    keepPreviousData: true,
  });

  const cabangs: Cabang[] = data?.Data ?? [];
  const totalCount: number = data?.TotalCount ?? 0;

  // =======================
  // SELECT
  // =======================
  useEffect(() => {
    setSelectAll(selectedIds.length === cabangs.length && cabangs.length > 0);
  }, [selectedIds, cabangs]);

  const toggleSelectAll = () => {
    const val = !selectAll;
    setSelectAll(val);
    setSelectedIds(val ? cabangs.map((x) => x.KdCabang) : []);
  };

  const toggleSelect = (id: string) => {
    setSelectedIds((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]
    );
  };

  // =======================
  // DELETE
  // =======================
  const handleConfirmDelete = async () => {
    try {
      for (const id of selectedIds) {
        await deleteCabang(id);
      }

      showSnackbar("Cabang berhasil dihapus", "success");

      setSelectedIds([]);
      setSelectAll(false);
      setOpenDeleteDialog(false);

      await mutate();
    } catch (err: any) {
      showSnackbar(err.message || "Gagal menghapus cabang", "error");
    }
  };

  // =======================
  // PAGINATION
  // =======================
  const handleChangePage = (_: any, newPage: number) => {
    setPage(newPage + 1);
  };

  const handleChangeRowsPerPage = (e: React.ChangeEvent<HTMLInputElement>) => {
    setPageSize(parseInt(e.target.value, 10));
    setPage(1);
  };

  // =======================
  // UI
  // =======================
  const loading = isLoading && !data;

  const headerStyle = (theme: any) => ({
		position: "relative",

		backgroundColor:
			theme.palette.mode === "dark"
				? theme.palette.background.paper
				: theme.palette.grey[100],

		background:
			theme.palette.mode === "dark"
				? alpha(theme.palette.background.paper, 0.9)
				: alpha(theme.palette.grey[200], 0.9),

		backdropFilter: "blur(6px)",
		WebkitBackdropFilter: "blur(6px)",

		backgroundClip: "padding-box", // 🔥 FIX kotak hitam

		color: theme.palette.text.primary,
		fontWeight: 600,

		borderBottom: `1px solid ${theme.palette.divider}`,
	});

  return (
    <Box>
      {/* 🔎 FILTER */}
      <Stack
        direction={{ xs: "column", sm: "row" }}
        spacing={2}
        mb={3}
        justifyContent="space-between"
      >
        <TextField
          placeholder="Search cabang"
          size="small"
          value={searchTerm}
          onChange={(e) => {
            setSearchTerm(e.target.value);
            setPage(1);
          }}
          InputProps={{
            endAdornment: (
              <InputAdornment position="end">
                <IconSearch size={16} />
              </InputAdornment>
            ),
          }}
        />

        <Stack direction="row" spacing={1}>
          {selectedIds.length > 0 && (
            <AccessButton
              access={{ subject: "MstCabang", action: "DeleteCabang" }}
              color="error"
              variant="outlined"
              onClick={() => setOpenDeleteDialog(true)}
            >
              Hapus
            </AccessButton>
          )}

          {editing && (
            <FormCabang
              cabangToEdit={editing}
              onClose={() => setEditing(null)}
              onSaved={mutate}
            />
          )}

          <AccessButton
            access={{ subject: "MstCabang", action: "PostCabang" }}
            color="primary"
            variant="contained"
            onClick={() => setEditing({} as Cabang)}
          >
            Tambah Cabang
          </AccessButton>
        </Stack>
      </Stack>

      {/* ================= TABLE ================= */}
      <TableContainer component={Paper} sx={{ maxHeight: "70vh", borderRadius: 1 }}>
        <Table size="small">
          <TableHead>
            <TableRow
							sx={(theme) => ({
								backgroundColor:
									theme.palette.mode === "dark" ? "#1f2937" : "#f3f4f6",
								"& th": {
									fontWeight: 600,
									fontSize: 13,
									color:
										theme.palette.mode === "dark" ? "#e5e7eb" : "#374151",
									borderBottom: "1px solid",
									borderColor: "divider",
								},
							})}
						>
              <TableCell padding="checkbox" sx={headerStyle}>
                <CustomCheckbox checked={selectAll} onChange={toggleSelectAll} />
              </TableCell>
              <TableCell sx={headerStyle}>Kode</TableCell>
              <TableCell sx={headerStyle}>Nama Cabang</TableCell>
              <TableCell sx={headerStyle}>Kota</TableCell>
              <TableCell sx={headerStyle}>Telepon</TableCell>
              <TableCell sx={headerStyle}>Grup</TableCell>
              <TableCell sx={headerStyle} align="right">
                Actions
              </TableCell>
            </TableRow>
          </TableHead>

          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={7} align="center">
                  <CircularProgress size={24} />
                </TableCell>
              </TableRow>
            ) : cabangs.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} align="center">
                  Data tidak ditemukan
                </TableCell>
              </TableRow>
            ) : (
              cabangs.map((item) => (
                <TableRow key={item.KdCabang}>
                  <TableCell padding="checkbox">
                    <CustomCheckbox
                      checked={selectedIds.includes(item.KdCabang)}
                      onChange={() => toggleSelect(item.KdCabang)}
                    />
                  </TableCell>

                  <TableCell>{item.KdCabang}</TableCell>
                  <TableCell>{item.NmCabang}</TableCell>
                  <TableCell>{item.Kota}</TableCell>
                  <TableCell>{item.Telepon}</TableCell>

                  <TableCell>
                    {item.Grup && (
                      <Chip label={item.Grup} size="small" />
                    )}
                  </TableCell>

                  <TableCell align="right">
                    <Tooltip title="Edit Cabang" arrow placement="top">
                      <span>
                        <AccessButton
                          access={{ subject: "MstCabang", action: "PutCabang" }}
                          color="success"
                          type="icon"
                          onClick={() => setEditing(item)}
                        >
                          <IconEdit width={18} />
                        </AccessButton>
                      </span>
                    </Tooltip>

                    <Tooltip title="Hapus cabang" arrow placement="top">
                      <span>
                        <AccessButton
                          access={{ subject: "MstCabang", action: "DeleteCabang" }}
                          color="error"
                          type="icon"
                          onClick={() => {
                            setSelectedIds([item.KdCabang]);
                            setOpenDeleteDialog(true);
                          }}
                        >
                          <IconTrash width={18} />
                        </AccessButton>
                      </span>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* ================= PAGINATION ================= */}
      <TablePagination
        component="div"
        count={totalCount}
        page={page - 1}
        onPageChange={handleChangePage}
        rowsPerPage={pageSize}
        onRowsPerPageChange={handleChangeRowsPerPage}
        rowsPerPageOptions={[5, 10, 25]}
        size="small"
      />

      {/* ================= DELETE ================= */}
      <Dialog open={openDeleteDialog} onClose={() => setOpenDeleteDialog(false)}>
        <DialogTitle>Konfirmasi Hapus</DialogTitle>
        <DialogContent>
          Apakah Anda yakin ingin menghapus data?
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDeleteDialog(false)}>Batal</Button>
          <Button color="error" onClick={handleConfirmDelete}>
            Hapus
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default CabangListComponent;