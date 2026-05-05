"use client"; // <-- wajib

import React from "react";
import Breadcrumb from "@/app/(DashboardLayout)/layout/shared/breadcrumb/Breadcrumb";
import PageContainer from "@/app/components/container/PageContainer";
// import AkunList from "@/app/components/konfigurasi/akun/akun-list/index";
import BlankCard from "@/app/components/shared/BlankCard";
import { CardContent } from "@mui/material";
import AkunListComponent from "@/app/feature/konfigurasi/akun";
import AuditLoginListComponent from "@/app/feature/konfigurasi/auditlogin";

const BCrumb = [
  {
    to: "/",
    title: "Home",
  },
  {
    title: "Daftar User Login",
  },
];

const AkunListing = () => {
  return (
        <PageContainer title="Daftar User Login" description="ini adalah daftar user login">
          <Breadcrumb title="Daftar User Login" items={BCrumb} />
          <BlankCard>
            <CardContent>
              <AuditLoginListComponent/>
            </CardContent>
          </BlankCard>
        </PageContainer>
  );
}
export default AkunListing;
