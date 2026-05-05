'use client';

import React, { useState } from 'react';
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
  CircularProgress,
  Chip,
  MenuItem,
  Typography,
  Tooltip,
  TableContainer,
  Paper,
  alpha,
} from '@mui/material';
import { IconSearch } from '@tabler/icons-react';
import useSWR from 'swr';
import dayjs from 'dayjs';
import { fetchAuditLogins } from '@/services/akun/audit-login.service';

const AuditLoginListComponent = () => {
  /* =======================
   * STATE
   * ======================= */
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  /* =======================
   * DATA FETCH
   * ======================= */
  const { data, isLoading } = useSWR(
    ['audit-login', searchTerm, statusFilter, page, pageSize],
    () =>
      fetchAuditLogins({
        username: searchTerm,
        isSuccess:
          statusFilter === 'all'
            ? undefined
            : statusFilter === 'success',
        page,
        pageSize,
      })
  );

  const logs = data?.Data ?? [];
  const totalCount: number = data?.TotalCount ?? 0;
  const loading = isLoading && !data;

  /* =======================
   * PAGINATION
   * ======================= */
  const handleChangePage = (_: unknown, newPage: number) =>
    setPage(newPage + 1);

  const handleChangeRowsPerPage = (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    setPageSize(parseInt(event.target.value, 10));
    setPage(1);
  };

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
  
  /* =======================
   * RENDER
   * ======================= */
  return (
    <Box>
      {/* FILTER SECTION */}
      <Stack
        direction={{ xs: 'column', sm: 'row' }}
        spacing={2}
        mb={3}
      >
        <TextField
          placeholder="Search username"
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

        <TextField
          select
          size="small"
          value={statusFilter}
          onChange={(e) => {
            setStatusFilter(e.target.value);
            setPage(1);
          }}
        >
          <MenuItem value="all">Semua</MenuItem>
          <MenuItem value="success">Sukses</MenuItem>
          <MenuItem value="failed">Gagal</MenuItem>
        </TextField>
      </Stack>

      {/* TABLE */}
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
              <TableCell sx={headerStyle}>User</TableCell>
              <TableCell sx={headerStyle}>Modul</TableCell>
              <TableCell sx={headerStyle}>Device</TableCell>
              <TableCell sx={headerStyle}>Lokasi</TableCell>
              <TableCell sx={headerStyle}>Login</TableCell>
              <TableCell sx={headerStyle}>Logout</TableCell>
              <TableCell sx={headerStyle}>Status</TableCell>
            </TableRow>
          </TableHead>

          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                  <CircularProgress size={24} />
                </TableCell>
              </TableRow>
            ) : logs.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
                  <Typography color="text.secondary">
                    Tidak ada data
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              logs.map((log: any) => (
                <TableRow
                  key={log.Id}
                  hover
                  sx={{
                    transition: "all 0.2s",
                    "&:hover": {
                      backgroundColor: "action.hover",
                    },
                    // 🔥 highlight gagal
                    ...(log.IsSuccess === false && {
                      backgroundColor: "rgba(255,0,0,0.03)",
                    }),
                  }}
                >
                  {/* USER */}
                  <TableCell>
                    <Typography fontSize={13} fontWeight={500}>
                      {log.Username}
                    </Typography>
                    <Typography fontSize={11} color="text.secondary">
                      {log.IpAddress}
                    </Typography>
                  </TableCell>

                  <TableCell>
                    <Typography fontSize={13} fontWeight={500}>
                      {log.Modul}
                    </Typography>
                    
                  </TableCell>

                  {/* DEVICE */}
                  <TableCell sx={{ maxWidth: 250 }}>
                    <Tooltip title={log.UserAgent || "-"}>
                      <Typography fontSize={13} noWrap>
                        {log.Device}
                      </Typography>
                    </Tooltip>
                    <Typography
                      fontSize={11}
                      color="text.secondary"
                      noWrap
                    >
                      {log.UserAgent || "-"}
                    </Typography>
                  </TableCell>

                  {/* LOKASI */}
                  <TableCell>
                    <Typography fontSize={13} fontWeight={500}>
                      {log.City || "-"}
                    </Typography>
                    <Typography fontSize={11} color="text.secondary">
                      {log.Country || ""}
                    </Typography>
                  </TableCell>

                  {/* LOGIN */}
                  <TableCell>
                    <Typography fontSize={13}>
                      {dayjs(log.LoginTime).format("DD-MM-YYYY")}
                    </Typography>
                    <Typography fontSize={11} color="text.secondary">
                      {dayjs(log.LoginTime).format("HH:mm")}
                    </Typography>
                  </TableCell>

                  {/* LOGOUT */}
                  <TableCell>
                    {log.LogoutTime ? (
                      <>
                        <Typography fontSize={13}>
                          {dayjs(log.LogoutTime).format("DD-MM-YYYY")}
                        </Typography>
                        <Typography fontSize={11} color="text.secondary">
                          {dayjs(log.LogoutTime).format("HH:mm")}
                        </Typography>
                      </>
                    ) : (
                      <Typography fontSize={12} color="text.disabled">
                        -
                      </Typography>
                    )}
                  </TableCell>

                  {/* STATUS */}
                  <TableCell>
                    <Chip
                      label={log.IsSuccess ? "Sukses" : "Gagal"}
                      color={log.IsSuccess ? "success" : "error"}
                      size="small"
                      sx={{ fontWeight: 500 }}
                    />
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>  
      </TableContainer>


      {/* PAGINATION */}
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
    </Box>
  );
};

export default AuditLoginListComponent;
