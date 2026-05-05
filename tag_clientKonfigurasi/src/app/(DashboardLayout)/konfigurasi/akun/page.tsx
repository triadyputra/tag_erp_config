"use client"; // <-- wajib

import React from "react";
import Breadcrumb from "@/app/(DashboardLayout)/layout/shared/breadcrumb/Breadcrumb";
import PageContainer from "@/app/components/container/PageContainer";
// import AkunList from "@/app/components/konfigurasi/akun/akun-list/index";
import BlankCard from "@/app/components/shared/BlankCard";
import { CardContent } from "@mui/material";
import AkunListComponent from "@/app/feature/konfigurasi/akun";

const BCrumb = [
  {
    to: "/",
    title: "Home",
  },
  {
    title: "Daftar Akun",
  },
];

const AkunListing = () => {
  return (
        <PageContainer title="Daftar Akun" description="ini adalah daftar akun aplikasi">
          <Breadcrumb title="Daftar Akun" items={BCrumb} />
          <BlankCard>
            <CardContent>
              <AkunListComponent/>
            </CardContent>
          </BlankCard>
        </PageContainer>
  );
}
export default AkunListing;
