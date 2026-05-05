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
  Typography,
} from "@mui/material";
import { IconSearch, IconTrash, IconEdit } from "@tabler/icons-react";
import CustomCheckbox from "@/app/components/forms/theme-elements/CustomCheckbox";

import { swrFetcher } from "@/utils/swrFetcher";
import useSWR from "swr";
import { useSnackbar } from "@/app/context/SnackbarContext";
import AccessButton from "@/app/components/buttons/AccessButton";
import { deleteMenu, saveMenu } from "@/services/akun/menu.service";
import FormMenu from "./FormMenu";

const BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
const API_URL = `${BASE_URL}MenuManagement`;

const MenuListComponent = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [selectAll, setSelectAll] = useState(false);
  const [openDeleteDialog, setOpenDeleteDialog] = useState(false);

  const { showSnackbar } = useSnackbar();

  const [editingMenu, setEditingMenu] = useState<any | null>(null);

  // =======================
  // SWR
  // =======================
  const url = `${API_URL}/GetListMenu?filter=${encodeURIComponent(
    searchTerm
  )}&page=${page}&pageSize=${pageSize}`;

  const { data, mutate, isLoading } = useSWR(url, swrFetcher, {
    revalidateOnFocus: true,
    keepPreviousData: true,
  });

  const menus = data?.Data ?? [];
  const totalCount = data?.TotalCount ?? 0;

  // =======================
  // SELECTION
  // =======================
  useEffect(() => {
    if (menus.length === 0) return;
    setSelectAll(selectedIds.length === menus.length);
  }, [selectedIds, menus]);

  const toggleSelectAll = () => {
    const value = !selectAll;
    setSelectAll(value);
    setSelectedIds(value ? menus.map((x: any) => x.IdMenu) : []);
  };

  const toggleSelect = (id: string) => {
    setSelectedIds((prev) =>
      prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]
    );
  };

  // =======================
  // SAVE
  // =======================
  const handleSaveMenu = async (payload: any) => {
    try {
      const isEdit = !editingMenu?.isNew;
      await saveMenu(payload, isEdit);

      showSnackbar(
        isEdit
          ? "Menu berhasil diperbarui"
          : "Menu berhasil ditambahkan",
        "success"
      );

      setEditingMenu(null);
      await mutate();
    } catch (err: any) {
      showSnackbar(err.message || "Gagal menyimpan menu", "error");
    }
  };

  // =======================
  // DELETE
  // =======================
  const handleDelete = async () => {
    try {
      for (const id of selectedIds) {
        await deleteMenu(id);
      }

      showSnackbar("Menu berhasil dihapus", "success");

      setSelectedIds([]);
      setSelectAll(false);
      setOpenDeleteDialog(false);

      await mutate();
    } catch (err: any) {
      showSnackbar(err.message || "Gagal menghapus menu", "error");
    }
  };

  // =======================
  // PAGINATION
  // =======================
  const handleChangePage = (_: unknown, newPage: number) =>
    setPage(newPage + 1);

  const handleChangeRowsPerPage = (event: any) => {
    setPageSize(parseInt(event.target.value, 10));
    setPage(1);
  };

  const loading = isLoading && !data;

  const headerStyle = (theme: any) => ({
    background:
      theme.palette.mode === "dark"
        ? alpha(theme.palette.background.paper, 0.9)
        : alpha(theme.palette.grey[200], 0.9),
    fontWeight: 600,
    fontSize: 13,
  });

  return (
    <Box>
      {/* ===================== HEADER ===================== */}
      <Stack
        direction={{ xs: "column", sm: "row" }}
        spacing={2}
        mb={3}
        justifyContent="space-between"
      >
        <TextField
          placeholder="Search menu"
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
              access={{ subject: "MenuManagement", action: "DeleteMenu" }}
              color="error"
              variant="outlined"
              onClick={() => setOpenDeleteDialog(true)}
            >
              Hapus
            </AccessButton>
          )}

          <AccessButton
            access={{ subject: "MenuManagement", action: "PostMenu" }}
            color="primary"
            variant="contained"
            onClick={() => setEditingMenu({ isNew: true })}
          >
            Tambah Menu
          </AccessButton>
        </Stack>
      </Stack>

      {/* ===================== TABLE ===================== */}
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell sx={headerStyle} padding="checkbox">
                <CustomCheckbox checked={selectAll} onChange={toggleSelectAll} />
              </TableCell>
              <TableCell sx={headerStyle}>Nama Menu</TableCell>
              <TableCell sx={headerStyle}>Modul</TableCell>
              <TableCell sx={headerStyle}>Controller</TableCell>
              <TableCell sx={headerStyle}>URL</TableCell>
              <TableCell sx={headerStyle}>Jumlah Action</TableCell>
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
            ) : menus.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} align="center">
                  Tidak ada data
                </TableCell>
              </TableRow>
            ) : (
              menus.map((menu: any) => (
                <TableRow key={menu.IdMenu}>
                  <TableCell padding="checkbox">
                    <CustomCheckbox
                      checked={selectedIds.includes(menu.IdMenu)}
                      onChange={() => toggleSelect(menu.IdMenu)}
                    />
                  </TableCell>

                  <TableCell>
                    <Stack>
                      <Typography fontWeight={600}>
                        {menu.NamaMenu}
                      </Typography>
                      {menu.Icon && (
                        <Typography fontSize={11} color="text.secondary">
                          Icon: {menu.Icon}
                        </Typography>
                      )}
                    </Stack>
                  </TableCell>

                  <TableCell>
                    <Chip
                      label={menu.Modul?.NamaModul || menu.IdModul}
                      size="small"
                      color="primary"
                      variant="outlined"
                    />
                  </TableCell>

                  <TableCell>
                    {menu.Controllers?.map((c: any, i: number) => (
                      <Chip
                        key={i}
                        label={c.NamaController}
                        size="small"
                        sx={{ mr: 0.5, mb: 0.5 }}
                      />
                    ))}
                  </TableCell>

                  <TableCell>
                    {menu.Controllers?.map((c: any, i: number) => (
                      <Typography key={i} fontSize={12}>
                        {c.Url}
                      </Typography>
                    ))}
                  </TableCell>

                  <TableCell>
                    {menu.Controllers?.reduce(
                      (total: number, c: any) =>
                        total + (c.Actions?.length || 0),
                      0
                    )}
                  </TableCell>

                  <TableCell align="right">
                    <Tooltip title="Edit">
                      <AccessButton
                        access={{ subject: "MenuManagement", action: "PutMenu" }}
                        color="success"
                        type="icon"
                        onClick={() => setEditingMenu(menu)}
                      >
                        <IconEdit size={18} />
                      </AccessButton>
                    </Tooltip>

                    <Tooltip title="Hapus">
                      <AccessButton
                        access={{ subject: "MenuManagement", action: "DeleteMenu" }}
                        color="error"
                        type="icon"
                        onClick={() => {
                          setSelectedIds([menu.IdMenu]);
                          setOpenDeleteDialog(true);
                        }}
                      >
                        <IconTrash size={18} />
                      </AccessButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* ===================== PAGINATION ===================== */}
      <TablePagination
        component="div"
        count={totalCount}
        page={page - 1}
        onPageChange={handleChangePage}
        rowsPerPage={pageSize}
        onRowsPerPageChange={handleChangeRowsPerPage}
      />

      {editingMenu && (
        <FormMenu
          menuToEdit={editingMenu}
          onClose={() => setEditingMenu(null)}
          onSubmit={handleSaveMenu}
        />
      )}

      {/* ===================== DELETE ===================== */}
      <Dialog open={openDeleteDialog} onClose={() => setOpenDeleteDialog(false)}>
        <DialogTitle>Konfirmasi</DialogTitle>
        <DialogContent>Yakin hapus data menu?</DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenDeleteDialog(false)}>Batal</Button>
          <Button color="error" onClick={handleDelete}>
            Hapus
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default MenuListComponent;